namespace Craftsman.Builders
{
    using Craftsman.Builders.Dtos;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class DbContextBuilder
    {
        public static void CreateDbContext(string solutionDirectory, ApiTemplate template)
        {
            try
            {
                var classPath = ClassPathHelper.DbContextClassPath(solutionDirectory, $"{template.DbContext.ContextName}.cs");

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = GetContextFileText(classPath.ClassNamespace, template);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                RegisterContext(solutionDirectory, template);

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        public static string GetContextFileText(string classNamespace, ApiTemplate template)
        {
            var nonAuth = @$"namespace {classNamespace}
{{
    using Application.Interfaces;
    using Domain.Entities;
    using Domain.Common;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using System.Threading;
    using System.Threading.Tasks;

    public class {template.DbContext.ContextName} : DbContext
    {{
        public {template.DbContext.ContextName}(
            DbContextOptions<{template.DbContext.ContextName}> options) : base(options) 
        {{
        }}

        #region DbSet Region - Do Not Delete
{GetDbSetText(template.Entities)}
        #endregion
    }}
}}";

            return template.AuthSetup.AuthMethod == "JWT" ? GetAuditableSaveOverride(classNamespace, template) : nonAuth;
        }

        public static string GetDbSetText(List<Entity> entities)
        {
            var dbSetText = "";

            foreach(var entity in entities)
            {
                var newLine = entity == entities.LastOrDefault() ? "" : $"{Environment.NewLine}";
                dbSetText += @$"        public DbSet<{entity.Name}> {entity.Plural} {{ get; set; }}{newLine}";
            }

            return dbSetText;
        }

        private static void RegisterContext(string solutionDirectory, ApiTemplate template)
        {
            var classPath = ClassPathHelper.InfraPersistenceServiceProviderClassPath(solutionDirectory, "ServiceRegistration.cs");

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var usingDbStatement = GetDbUsingStatement(template.DbContext.Provider);
            InstallDbProviderNugetPackages(template.DbContext.Provider, solutionDirectory);

            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using (var output = new StreamWriter(tempPath))
                {
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains("#region DbContext"))
                        {
                            newText += @$"          
            if (configuration.GetValue<bool>(""UseInMemoryDatabase""))
            {{
                services.AddDbContext<{template.DbContext.ContextName}>(options =>
                    options.UseInMemoryDatabase($""{template.DbContext.DatabaseName ?? template.DbContext.ContextName}""));
            }}
            else
            {{
                services.AddDbContext<{template.DbContext.ContextName}>(options =>
                    options.{usingDbStatement}(
                        configuration.GetConnectionString(""{template.DbContext.DatabaseName}""),
                        builder => builder.MigrationsAssembly(typeof({template.DbContext.ContextName}).Assembly.FullName)));
            }}";
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);

            GlobalSingleton.AddUpdatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
        }

        private static void InstallDbProviderNugetPackages(string provider, string solutionDirectory)
        {
            var installCommand = $"add Infrastructure.Persistence{Path.DirectorySeparatorChar}Infrastructure.Persistence.csproj package npgsql.entityframeworkcore.postgresql";

            if (Enum.GetName(typeof(DbProvider), DbProvider.Postgres) == provider)
                installCommand = $"add Infrastructure.Persistence{Path.DirectorySeparatorChar}Infrastructure.Persistence.csproj package npgsql.entityframeworkcore.postgresql";
            //else if (Enum.GetName(typeof(DbProvider), DbProvider.MySql) == provider)
            //    installCommand = $"add Infrastructure.Persistence{Path.DirectorySeparatorChar}Infrastructure.Persistence.csproj package Pomelo.EntityFrameworkCore.MySql";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = installCommand,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = solutionDirectory
                }
            };

            process.Start();
            process.WaitForExit();
        }

        private static object GetDbUsingStatement(string provider)
        {
            if (Enum.GetName(typeof(DbProvider), DbProvider.Postgres) == provider)
                return "UseNpgsql";
            //else if (Enum.GetName(typeof(DbProvider), DbProvider.MySql) == provider)
            //    return "UseMySql";
            
            return "UseSqlServer";
        }

        public static string GetAuditableSaveOverride(string classNamespace, ApiTemplate template)
        {
            return @$"namespace {classNamespace}
{{
    using Application.Interfaces;
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain.Common;
    using System.Reflection;

    public class {template.DbContext.ContextName} : DbContext
    {{
        private readonly IDateTimeService _dateTimeService;
        private readonly ICurrentUserService _currentUserService;

        public {template.DbContext.ContextName}(
            DbContextOptions<{template.DbContext.ContextName}> options,
            ICurrentUserService currentUserService,
            IDateTimeService dateTimeService) : base(options) 
        {{
            _currentUserService = currentUserService;
            _dateTimeService = dateTimeService;
        }}

        #region DbSet Region - Do Not Delete
{GetDbSetText(template.Entities)}
        #endregion

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {{
            foreach (EntityEntry<AuditableEntity> entry in ChangeTracker.Entries<AuditableEntity>())
            {{
                switch (entry.State)
                {{
                    case EntityState.Added:
                        entry.Entity.CreatedBy = _currentUserService.UserId;
                        entry.Entity.CreatedOn = _dateTimeService.NowUtc;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastModifiedBy = _currentUserService.UserId;
                        entry.Entity.LastModifiedOn = _dateTimeService.NowUtc;
                        break;
                }}
            }}

            int result = await base.SaveChangesAsync(cancellationToken);

            return result;
        }}

        public override int SaveChanges()
        {{
            foreach (EntityEntry<AuditableEntity> entry in ChangeTracker.Entries<AuditableEntity>())
            {{
                switch (entry.State)
                {{
                    case EntityState.Added:
                        entry.Entity.CreatedBy = _currentUserService.UserId;
                        entry.Entity.CreatedOn = _dateTimeService.NowUtc;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastModifiedBy = _currentUserService.UserId;
                        entry.Entity.LastModifiedOn = _dateTimeService.NowUtc;
                        break;
                }}
            }}

            int result = base.SaveChanges();

            return result;
        }}

        protected override void OnModelCreating(ModelBuilder builder)
        {{
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(builder);
        }}
    }}
}}";
        }
    }
}
