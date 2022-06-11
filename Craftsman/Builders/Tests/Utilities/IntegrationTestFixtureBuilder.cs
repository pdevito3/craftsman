namespace Craftsman.Builders.Tests.Utilities;

using Domain;
using Helpers;
using Services;

public class IntegrationTestFixtureBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public IntegrationTestFixtureBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateFixture(string testDirectory, string srcDirectory, string projectBaseName, string dbContextName, DbProvider provider)
    {
        var classPath = ClassPathHelper.IntegrationTestProjectRootClassPath(testDirectory, "TestFixture.cs", projectBaseName);
        var fileText = GetFixtureText(classPath.ClassNamespace, testDirectory, srcDirectory, projectBaseName, dbContextName, provider);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetFixtureText(string classNamespace, string testDirectory, string srcDirectory, string projectBaseName, string dbContextName, DbProvider provider)
    {
        var apiClassPath = ClassPathHelper.WebApiProjectClassPath(srcDirectory, projectBaseName);
        var contextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
        var utilsClassPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "", projectBaseName);
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        var configClassPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, "", projectBaseName);

        var usingStatement = provider == DbProvider.Postgres
            ? $@"
using Npgsql;"
            : null;

        var checkpoint = provider == DbProvider.Postgres
            ? $@"_checkpoint = new Checkpoint
        {{
            TablesToIgnore = new Table[] {{ ""__EFMigrationsHistory"" }},
            SchemasToExclude = new[] {{ ""information_schema"", ""pg_subscription"", ""pg_catalog"", ""pg_toast"" }},
            DbAdapter = DbAdapter.Postgres
        }};"
            : $@"_checkpoint = new Checkpoint
        {{
            TablesToIgnore = new Table[] {{ ""__EFMigrationsHistory"" }},
        }};";

        var resetString = provider == DbProvider.Postgres
            ? $@"using var conn = new NpgsqlConnection(Environment.GetEnvironmentVariable(""DB_CONNECTION_STRING""));
        await conn.OpenAsync();
        await _checkpoint.Reset(conn);"
            : $@"await _checkpoint.Reset(Environment.GetEnvironmentVariable(""DB_CONNECTION_STRING""));";

        return @$"namespace {classNamespace};

using {configClassPath.ClassNamespace};
using {contextClassPath.ClassNamespace};
using {apiClassPath.ClassNamespace};
using {utilsClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;{usingStatement}
using Xunit;
using Respawn;
using Respawn.Graph;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Configurations.Databases;
using DotNet.Testcontainers.Containers.Modules.Abstractions;
using DotNet.Testcontainers.Containers.Modules.Databases;

[CollectionDefinition(nameof(TestFixture))]
public class TestFixtureCollection : ICollectionFixture<TestFixture> {{ }}

public class TestFixture : IAsyncLifetime
{{
    private static IServiceScopeFactory _scopeFactory;
    private static Checkpoint _checkpoint;
    private static ServiceProvider _provider;
    private readonly TestcontainerDatabase _dbContainer = dbSetup();

    public async Task InitializeAsync()
    {{
        await _dbContainer.StartAsync();
        {provider.IntegrationTestConnectionStringSetup()}
        
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
        {{
            EnvironmentName = LocalConfig.IntegrationTestingEnvName,
        }});
        builder.Configuration.AddEnvironmentVariables();

        builder.ConfigureServices();
        var services = builder.Services;

        // add any mock services here
        // services.ReplaceServiceWithSingletonMock<IHttpContextAccessor>();

        // MassTransit Harness Setup -- Do Not Delete Comment

        _provider = services.BuildServiceProvider();
        _scopeFactory = _provider.GetService<IServiceScopeFactory>();

        // MassTransit Start Setup -- Do Not Delete Comment

        {checkpoint}

        await EnsureDatabase();
    }}

    private static async Task EnsureDatabase()
    {{
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetService<{dbContextName}>();

        await context?.Database?.MigrateAsync();
        await ResetState();
    }}

    public static TScopedService GetService<TScopedService>()
    {{
        var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetService<TScopedService>();
        return service;
    }}


    public static void SetUserRole(string role, string sub = null)
    {{
        sub ??= Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {{
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.Name, sub)
        }};

        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = Mock.Of<HttpContext>(c => c.User == claimsPrincipal);

        var httpContextAccessor = GetService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = httpContext;
    }}

    public static void SetUserRoles(string[] roles, string sub = null)
    {{
        sub ??= Guid.NewGuid().ToString();
        var claims = new List<Claim>();
        foreach (var role in roles)
        {{
            claims.Add(new Claim(ClaimTypes.Role, role));
        }}
        claims.Add(new Claim(ClaimTypes.Name, sub));

        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = Mock.Of<HttpContext>(c => c.User == claimsPrincipal);

        var httpContextAccessor = GetService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = httpContext;
    }}
    
    public static void SetMachineRole(string role, string clientId = null)
    {{
        clientId ??= Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {{
            new Claim(""client_role"", role),
            new Claim(""client_id"", clientId)
        }};

        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = Mock.Of<HttpContext>(c => c.User == claimsPrincipal);

        var httpContextAccessor = GetService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = httpContext;
    }}

    public static void SetMachineRoles(string[] roles, string clientId = null)
    {{
        clientId ??= Guid.NewGuid().ToString();
        var claims = new List<Claim>();
        foreach (var role in roles)
        {{
            claims.Add(new Claim(""client_role"", role));
        }}
        claims.Add(new Claim(""client_id"", clientId));

        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = Mock.Of<HttpContext>(c => c.User == claimsPrincipal);

        var httpContextAccessor = GetService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = httpContext;
    }}

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {{
        using var scope = _scopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetService<ISender>();

        return await mediator.Send(request);
    }}

    public static async Task ResetState()
    {{
        {resetString}
    }}

    public static async Task<TEntity> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {{
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetService<{dbContextName}>();

        return await context.FindAsync<TEntity>(keyValues);
    }}

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {{
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetService<{dbContextName}>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }}

    public static async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
    {{
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<{dbContextName}>();

        try
        {{
            //await dbContext.BeginTransactionAsync();

            await action(scope.ServiceProvider);

            //await dbContext.CommitTransactionAsync();
        }}
        catch (Exception)
        {{
            //dbContext.RollbackTransaction();
            throw;
        }}
    }}

    public static async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {{
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<{dbContextName}>();

        try
        {{
            //await dbContext.BeginTransactionAsync();

            var result = await action(scope.ServiceProvider);

            //await dbContext.CommitTransactionAsync();

            return result;
        }}
        catch (Exception)
        {{
            //dbContext.RollbackTransaction();
            throw;
        }}
    }}

    public static Task ExecuteDbContextAsync(Func<{dbContextName}, Task> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<{dbContextName}>()));

    public static Task ExecuteDbContextAsync(Func<{dbContextName}, ValueTask> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<{dbContextName}>()).AsTask());

    public static Task ExecuteDbContextAsync(Func<{dbContextName}, IMediator, Task> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<{dbContextName}>(), sp.GetService<IMediator>()));

    public static Task<T> ExecuteDbContextAsync<T>(Func<{dbContextName}, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<{dbContextName}>()));

    public static Task<T> ExecuteDbContextAsync<T>(Func<{dbContextName}, ValueTask<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<{dbContextName}>()).AsTask());

    public static Task<T> ExecuteDbContextAsync<T>(Func<{dbContextName}, IMediator, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<{dbContextName}>(), sp.GetService<IMediator>()));

    public static Task<int> InsertAsync<T>(params T[] entities) where T : class
    {{
        return ExecuteDbContextAsync(db =>
        {{
            foreach (var entity in entities)
            {{
                db.Set<T>().Add(entity);
            }}
            return db.SaveChangesAsync();
        }});
    }}

    // MassTransit Methods -- Do Not Delete Comment

    {provider.IntegrationTestDbSetupMethod(projectBaseName)}

    public async Task DisposeAsync()
    {{
        await _dbContainer.DisposeAsync();
        // MassTransit Teardown -- Do Not Delete Comment
    }}
}}

public static class ServiceCollectionServiceExtensions
{{
    public static IServiceCollection ReplaceServiceWithSingletonMock<TService>(this IServiceCollection services)
        where TService : class
    {{
        services.RemoveAll(typeof(TService));
        services.AddSingleton(_ => Mock.Of<TService>());
        return services;
    }}
}}
";
    }
}
