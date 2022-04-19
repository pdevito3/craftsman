namespace NewCraftsman.Builders.Tests.Utilities
{
    using System;
    using System.IO.Abstractions;

    public class IntegrationTestFixtureBuilder
    {
        public static void CreateFixture(string solutionDirectory, string projectBaseName, string dbContextName, string dbName, string provider, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.IntegrationTestProjectRootClassPath(solutionDirectory, "TestFixture.cs", projectBaseName);
            var fileText = GetFixtureText(classPath.ClassNamespace, solutionDirectory, projectBaseName, dbContextName, dbName, provider);
            _utilities.CreateFile(classPath, fileText);
        }

        public static string GetFixtureText(string classNamespace, string solutionDirectory, string projectBaseName, string dbContextName, string dbName, string provider)
        {
            var apiClassPath = ClassPathHelper.WebApiProjectClassPath(solutionDirectory, projectBaseName);
            var contextClassPath = ClassPathHelper.DbContextClassPath(solutionDirectory, "", projectBaseName);
            var testUtilsClassPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(solutionDirectory, projectBaseName, "");
            var utilsClassPath = ClassPathHelper.WebApiResourcesClassPath(solutionDirectory, "", projectBaseName);
            var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(solutionDirectory, "", projectBaseName);

            var usingStatement = Enum.GetName(typeof(DbProvider), DbProvider.Postgres) == provider
                ? $@"
using Npgsql;"
                : null;

            var checkpoint = Enum.GetName(typeof(DbProvider), DbProvider.Postgres) == provider
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

            var resetString = Enum.GetName(typeof(DbProvider), DbProvider.Postgres) == provider
                ? $@"using var conn = new NpgsqlConnection(Environment.GetEnvironmentVariable(""DB_CONNECTION_STRING""));
        await conn.OpenAsync();
        await _checkpoint.Reset(conn);"
                : $@"await _checkpoint.Reset(Environment.GetEnvironmentVariable(""DB_CONNECTION_STRING""));";

            return @$"namespace {classNamespace};

using {contextClassPath.ClassNamespace};
using {testUtilsClassPath.ClassNamespace};
using {apiClassPath.ClassNamespace};
using {utilsClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;{usingStatement}
using NUnit.Framework;
using Respawn;
using Respawn.Graph;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

[SetUpFixture]
public class TestFixture
{{
    private static IConfigurationRoot _configuration;
    private static IWebHostEnvironment _env;
    private static IServiceScopeFactory _scopeFactory;
    private static Checkpoint _checkpoint;
    private static ServiceProvider _provider;

    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {{
        var dockerDbPort = await DockerDatabaseUtilities.EnsureDockerStartedAndGetPortPortAsync();
        var dockerConnectionString = DockerDatabaseUtilities.GetSqlConnectionString(dockerDbPort.ToString());
        Environment.SetEnvironmentVariable(""DB_CONNECTION_STRING"", dockerConnectionString);

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddInMemoryCollection(new Dictionary<string, string> {{ }})
            .AddEnvironmentVariables();

        _configuration = builder.Build();
        _env = Mock.Of<IWebHostEnvironment>(e => e.EnvironmentName == LocalConfig.IntegrationTestingEnvName);

        var startup = new Startup(_configuration, _env);

        var services = new ServiceCollection();

        services.AddLogging();

        startup.ConfigureServices(services);

        // add any mock services here
        var httpContextAccessorService = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IHttpContextAccessor));
        services.Remove(httpContextAccessorService);
        services.AddSingleton(_ => Mock.Of<IHttpContextAccessor>());

        // MassTransit Harness Setup -- Do Not Delete Comment

        _provider = services.BuildServiceProvider();
        _scopeFactory = _provider.GetService<IServiceScopeFactory>();

        // MassTransit Start Setup -- Do Not Delete Comment

        {checkpoint}

        EnsureDatabase();
    }}

    private static void EnsureDatabase()
    {{
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetService<{dbContextName}>();

        context.Database.Migrate();
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

    [OneTimeTearDown]
    public async Task RunAfterAnyTests()
    {{
        // MassTransit Teardown -- Do Not Delete Comment
    }}
}}";
        }
    }
}