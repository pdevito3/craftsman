namespace Craftsman.Builders;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Domain;
using Helpers;
using Services;

public class DbContextBuilder
{
    private readonly ICraftsmanUtilities _utilities;
    private readonly IFileSystem _fileSystem;

    public DbContextBuilder(ICraftsmanUtilities utilities, IFileSystem fileSystem)
    {
        _utilities = utilities;
        _fileSystem = fileSystem;
    }

    public void CreateDbContext(string srcDirectory,
        List<Entity> entities,
        string dbContextName,
        DbProvider dbProvider,
        string dbName,
        string localDbConnection,
        NamingConventionEnum namingConventionEnum,
        bool useSoftDelete,
        string projectBaseName
    )
    {
        var classPath = ClassPathHelper.DbContextClassPath(srcDirectory, $"{dbContextName}.cs", projectBaseName);
        var data = GetContextFileText(classPath.ClassNamespace, entities, dbContextName, srcDirectory, useSoftDelete, projectBaseName, dbProvider);
        _utilities.CreateFile(classPath, data);

        RegisterContext(srcDirectory, dbProvider, dbContextName, dbName, localDbConnection, namingConventionEnum, projectBaseName);
    }

    public static string GetContextFileText(string classNamespace, List<Entity> entities, string dbContextName, string srcDirectory, bool useSoftDelete, string projectBaseName, DbProvider dbProvider)
    {
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        var baseEntityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, $"", "", projectBaseName);
        var entityConfigClassPath = ClassPathHelper.DatabaseConfigClassPath(srcDirectory, $"", projectBaseName);

        var softDelete = useSoftDelete
            ? $@"
                    entry.State = EntityState.Modified;
                    entry.Entity.UpdateModifiedProperties(now, currentUserService?.UserId);
                    entry.Entity.UpdateIsDeleted(true);"
            : "";
        var softDeleteFilterClass = useSoftDelete
            ? $@"

{SoftDeleteFilterClass()}"
            : "";

        var modelBuilderFilter = useSoftDelete
            ? $@"
        modelBuilder.FilterSoftDeletedRecords();
        /* any query filters added after this will override soft delete 
                https://docs.microsoft.com/en-us/ef/core/querying/filters
                https://github.com/dotnet/efcore/issues/10275
        */"
            : "";

        return @$"namespace {classNamespace};

using {baseEntityClassPath.ClassNamespace};
using {entityConfigClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using Configurations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

public sealed class {dbContextName}(DbContextOptions<{dbContextName}> options, 
    ICurrentUserService currentUserService, 
    IMediator mediator, 
    TimeProvider dateTimeProvider)
    : DbContext(options)
{{
    #region DbSet Region - Do Not Delete
    #endregion DbSet Region - Do Not Delete

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {{
        base.OnModelCreating(modelBuilder);{modelBuilderFilter}

        #region Entity Database Config Region - Only delete if you don't want to automatically add configurations
        #endregion Entity Database Config Region - Only delete if you don't want to automatically add configurations
    }}

    public override int SaveChanges()
    {{
        UpdateAuditFields();
        var result = base.SaveChanges();
        _dispatchDomainEvents().GetAwaiter().GetResult();
        return result;
    }}

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {{
        UpdateAuditFields();
        var result = await base.SaveChangesAsync(cancellationToken);
        await _dispatchDomainEvents();
        return result;
    }}
    
    private async Task _dispatchDomainEvents()
    {{
        var domainEventEntities = ChangeTracker.Entries<BaseEntity>()
            .Select(po => po.Entity)
            .Where(po => po.DomainEvents.Any())
            .ToArray();

        foreach (var entity in domainEventEntities)
        {{
            var events = entity.DomainEvents.ToArray();
            entity.DomainEvents.Clear();
            foreach (var entityDomainEvent in events)
                await mediator.Publish(entityDomainEvent);
        }}
    }}
        
    private void UpdateAuditFields()
    {{
        var now = dateTimeProvider.GetUtcNow();
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {{
            switch (entry.State)
            {{
                case EntityState.Added:
                    entry.Entity.UpdateCreationProperties(now, currentUserService?.UserId);
                    entry.Entity.UpdateModifiedProperties(now, currentUserService?.UserId);
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdateModifiedProperties(now, currentUserService?.UserId);
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

    private void RegisterContext(string srcDirectory, DbProvider dbProvider, string dbContextName, string dbName, string localDbConnection, NamingConventionEnum namingConventionEnum, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"{FileNames.GetInfraRegistrationName()}.cs", projectBaseName);

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");
        InstallDbProviderNugetPackages(dbProvider, srcDirectory);

        //TODO test for class and another for anything else
        var namingConvention = namingConventionEnum == NamingConventionEnum.Class
            ? ""
            : @$"
                            .{namingConventionEnum.ExtensionMethod()}()";

        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains("// DbContext -- Do Not Delete")) // abstract this to a constants file?
                    {
                        newText += @$"
        var connectionString = configuration.GetConnectionStringOptions().{CraftsmanUtilities.GetCleanProjectName(projectBaseName)};
        if(string.IsNullOrWhiteSpace(connectionString))
        {{
            // this makes local migrations easier to manage. feel free to refactor if desired.
            connectionString = env.IsDevelopment() 
                ? ""{localDbConnection}""
                : throw new Exception(""The database connection string is not set."");
        }}

        services.AddDbContext<{dbContextName}>(options =>
            options.{dbProvider.DbRegistrationStatement()}(connectionString,
                builder => builder.MigrationsAssembly(typeof({dbContextName}).Assembly.FullName)){namingConvention});

        services.AddHostedService<MigrationHostedService<{dbContextName}>>();";
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }

    private static void InstallDbProviderNugetPackages(DbProvider provider, string srcDirectory)
    {
        var installCommand = $"add Infrastructure.Persistence{Path.DirectorySeparatorChar}Infrastructure.Persistence.csproj package Microsoft.EntityFrameworkCore.SqlServer --version 5.0.0";

        if (DbProvider.Postgres == provider)
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
}
