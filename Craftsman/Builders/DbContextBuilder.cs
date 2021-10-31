namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text;

    public class DbContextBuilder
    {
        public static void CreateDbContext(string srcDirectory,
            List<Entity> entities,
            string dbContextName,
            string dbProvider,
            string dbName,
            NamingConventionEnum namingConventionEnum,
            string projectBaseName,
            IFileSystem fileSystem
        )
        {
            var classPath = ClassPathHelper.DbContextClassPath(srcDirectory, $"{dbContextName}.cs", projectBaseName);
            var data = GetContextFileText(classPath.ClassNamespace, entities, dbContextName, srcDirectory, projectBaseName);
            Utilities.CreateFile(classPath, data, fileSystem);
            
            RegisterContext(srcDirectory, dbProvider, dbContextName, dbName, namingConventionEnum, projectBaseName);
        }

        public static string GetContextFileText(string classNamespace, List<Entity> entities, string dbContextName, string solutionDirectory, string projectBaseName)
        {
            var entitiesUsings = "";
            foreach (var entity in entities)
            {
                var classPath = ClassPathHelper.EntityClassPath(solutionDirectory, "", entity.Plural, projectBaseName);
                entitiesUsings += $"{Environment.NewLine}using {classPath.ClassNamespace};";
            }
            
            return @$"namespace {classNamespace};{entitiesUsings}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading;
using System.Threading.Tasks;

public class {dbContextName} : DbContext
{{
    public {dbContextName}(
        DbContextOptions<{dbContextName}> options) : base(options)
    {{
    }}

    #region DbSet Region - Do Not Delete

{GetDbSetText(entities)}
    #endregion DbSet Region - Do Not Delete

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {{
    }}
}}";
        }

        public static string GetDbSetText(List<Entity> entities)
        {
            var dbSetText = "";

            foreach (var entity in entities)
            {
                var newLine = entity == entities.LastOrDefault() ? "" : $"{Environment.NewLine}";
                dbSetText += @$"    public DbSet<{entity.Name}> {entity.Plural} {{ get; set; }}{newLine}";
            }

            return dbSetText;
        }

        private static void RegisterContext(string srcDirectory, string dbProvider, string dbContextName, string dbName, NamingConventionEnum namingConventionEnum, string projectBaseName)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"{Utilities.GetInfraRegistrationName()}.cs", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var usingDbStatement = GetDbUsingStatement(dbProvider);
            InstallDbProviderNugetPackages(dbProvider, srcDirectory);

            //TODO test for class and another for anything else
            var namingConvention = namingConventionEnum == NamingConventionEnum.Class
                ? ""
                : @$".{namingConventionEnum.ExtensionMethod()}()
                            ";
            
            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using (var output = new StreamWriter(tempPath))
                {
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains("// DbContext -- Do Not Delete")) // abstract this to a constants file?
                        {
                            newText += @$"
            if (env.IsEnvironment(LocalConfig.FunctionalTestingEnvName) || env.IsDevelopment())
            {{
                services.AddDbContext<{dbContextName}>(options =>
                    options.UseInMemoryDatabase($""{dbName ?? dbContextName}""));
            }}
            else
            {{
                services.AddDbContext<{dbContextName}>(options =>
                    options.{usingDbStatement}(
                        configuration.GetConnectionString(""{dbName}""),
                        builder => builder.MigrationsAssembly(typeof({dbContextName}).Assembly.FullName)){namingConvention});
            }}";
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);
        }

        private static void InstallDbProviderNugetPackages(string provider, string solutionDirectory)
        {
            var installCommand = $"add Infrastructure.Persistence{Path.DirectorySeparatorChar}Infrastructure.Persistence.csproj package Microsoft.EntityFrameworkCore.SqlServer --version 5.0.0";

            if (Enum.GetName(typeof(DbProvider), DbProvider.Postgres) == provider)
                installCommand = $"add Infrastructure.Persistence{Path.DirectorySeparatorChar}Infrastructure.Persistence.csproj package npgsql.entityframeworkcore.postgresql  --version 5.0.0";
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

        public static string GetAuditableSaveOverride(string classNamespace, List<Entity> entities, string dbContextName, string solutionDirectory, string projectBaseName)
        {
            var entitiesUsings = "";
            foreach (var entity in entities)
            {
                var classPath = ClassPathHelper.EntityClassPath(solutionDirectory, "", entity.Plural, projectBaseName);
                entitiesUsings += $"{Environment.NewLine}using {classPath.ClassNamespace};";
            }
            // notice domain.common that would need to be added and looked up. possibly interfaces too
            return @$"namespace {classNamespace};{entitiesUsings};
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading;
using System.Threading.Tasks;
using Domain.Common;
using System.Reflection;

public class {dbContextName} : DbContext
{{
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUserService;

    public {dbContextName}(
        DbContextOptions<{dbContextName}> options,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService) : base(options)
    {{
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
    }}

    #region DbSet Region - Do Not Delete

{GetDbSetText(entities)}
    #endregion DbSet Region - Do Not Delete

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
}}";
        }
    }
}