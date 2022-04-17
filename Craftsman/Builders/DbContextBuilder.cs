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
            string localDbConnection,
            NamingConventionEnum namingConventionEnum,
            bool useSoftDelete,
            string projectBaseName,
            IFileSystem fileSystem
        )
        {
            var classPath = ClassPathHelper.DbContextClassPath(srcDirectory, $"{dbContextName}.cs", projectBaseName);
            var data = GetContextFileText(classPath.ClassNamespace, entities, dbContextName, srcDirectory, useSoftDelete, projectBaseName);
            Utilities.CreateFile(classPath, data, fileSystem);
            
            RegisterContext(srcDirectory, dbProvider, dbContextName, dbName, localDbConnection, namingConventionEnum, projectBaseName);
        }

        public static string GetContextFileText(string classNamespace, List<Entity> entities, string dbContextName, string srcDirectory, bool useSoftDelete, string projectBaseName)
        {
            var entitiesUsings = "";
            foreach (var entity in entities)
            {
                var classPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
                entitiesUsings += $"{Environment.NewLine}using {classPath.ClassNamespace};";
            }
            var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
            var baseEntityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, $"", "", projectBaseName);

            var softDelete = useSoftDelete 
                ? $@"
                    entry.State = EntityState.Modified;
                    entry.Entity.UpdateModifiedProperties(now, _currentUserService?.UserId);
                    entry.Entity.UpdateIsDeleted(true);"
                : "";            
            var softDeleteFilterClass = useSoftDelete 
                ? $@"

{SoftDeleteFilterClass()}"
                :"";
            
            var modelBuilderFilter = useSoftDelete 
                ? $@"
            modelBuilder.FilterSoftDeletedRecords();"
                : "";
            
            return @$"namespace {classNamespace};

{entitiesUsings}
using {baseEntityClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

public class {dbContextName} : DbContext
{{
    private readonly ICurrentUserService _currentUserService;

    public {dbContextName}(
        DbContextOptions<{dbContextName}> options, ICurrentUserService currentUserService) : base(options)
    {{
        _currentUserService = currentUserService;
    }}

    #region DbSet Region - Do Not Delete

{GetDbSetText(entities)}
    #endregion DbSet Region - Do Not Delete

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {{
        base.OnModelCreating(modelBuilder);{modelBuilderFilter}
    }}

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {{
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }}

    public override int SaveChanges()
    {{
        UpdateAuditFields();
        return base.SaveChanges();
    }}
        
    private void UpdateAuditFields()
    {{
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {{
            switch (entry.State)
            {{
                case EntityState.Added:
                    entry.Entity.UpdateCreationProperties(now, _currentUserService?.UserId);
                    entry.Entity.UpdateModifiedProperties(now, _currentUserService?.UserId);
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdateModifiedProperties(now, _currentUserService?.UserId);
                    break;
                
                case EntityState.Deleted:{softDelete}
                    break;
            }}
        }}
    }}
}}{softDeleteFilterClass}";
        }

        private static string SoftDeleteFilterClass()
        {
            return $@"public static class Extensions
{{
    public static void FilterSoftDeletedRecords(this ModelBuilder modelBuilder)
    {{
        Expression<Func<BaseEntity, bool>> filterExpr = e => !e.IsDeleted;
        foreach (var mutableEntityType in modelBuilder.Model.GetEntityTypes()
            .Where(m => m.ClrType.IsAssignableTo(typeof(BaseEntity))))
        {{
            // modify expression to handle correct child type
            var parameter = Expression.Parameter(mutableEntityType.ClrType);
            var body = ReplacingExpressionVisitor
                .Replace(filterExpr.Parameters.First(), parameter, filterExpr.Body);
            var lambdaExpression = Expression.Lambda(body, parameter);

            // set filter
            mutableEntityType.SetQueryFilter(lambdaExpression);
        }}
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

        private static void RegisterContext(string srcDirectory, string dbProvider, string dbContextName, string dbName, string localDbConnection, NamingConventionEnum namingConventionEnum, string projectBaseName)
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
                : @$"
                            .{namingConventionEnum.ExtensionMethod()}()";
            
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
        if (env.IsEnvironment(LocalConfig.FunctionalTestingEnvName))
        {{
            services.AddDbContext<{dbContextName}>(options =>
                options.UseInMemoryDatabase($""{dbName ?? dbContextName}""));
        }}
        else
        {{
            var connectionString = Environment.GetEnvironmentVariable(""DB_CONNECTION_STRING"");
            if(string.IsNullOrEmpty(connectionString))
            {{
                // this makes local migrations easier to manage. feel free to refactor if desired.
                connectionString = env.IsDevelopment() 
                    ? ""{localDbConnection}""
                    : throw new Exception(""DB_CONNECTION_STRING environment variable is not set."");
            }}

            services.AddDbContext<{dbContextName}>(options =>
                options.{usingDbStatement}(connectionString,
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

        private static void InstallDbProviderNugetPackages(string provider, string srcDirectory)
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
                    WorkingDirectory = srcDirectory
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
    }
}