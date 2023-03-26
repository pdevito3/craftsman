namespace Craftsman.Builders.Tests.Utilities;

using Helpers;
using Services;

public class FunctionalTestBaseBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public FunctionalTestBaseBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateBase(string srcDirectory, string testDirectory, string projectBaseName, string dbContextName, bool hasAuth)
    {
        var classPath = ClassPathHelper.FunctionalTestProjectRootClassPath(testDirectory, "TestBase.cs", projectBaseName);
        var fileText = GetBaseText(classPath.ClassNamespace, srcDirectory, testDirectory, projectBaseName, dbContextName, hasAuth);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetBaseText(string classNamespace, string srcDirectory, string testDirectory, string projectBaseName, string dbContextName, bool hasAuth)
    {
        var contextClassPath = ClassPathHelper.DbContextClassPath(testDirectory, "", projectBaseName);
        var apiClassPath = ClassPathHelper.WebApiProjectRootClassPath(testDirectory, "", projectBaseName);
        var rolesEntityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "Roles", projectBaseName);
        var usersEntityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "Users", projectBaseName);
        var fakeUserEntityClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", "User", projectBaseName);

        var authUsings = hasAuth ? $@"
using {rolesEntityClassPath.ClassNamespace};
using {usersEntityClassPath.ClassNamespace};
using {fakeUserEntityClassPath.ClassNamespace};" : null;

        var authHelperMethods = hasAuth
            ? $@"

    public static async Task<User> AddNewSuperAdmin()
    {{
        var user = new FakeUserBuilder().Build();
        user.AddRole(Role.SuperAdmin());
        await InsertAsync(user);
        return user;
    }}

    public static async Task<User> AddNewUser(List<Role> roles)
    {{
        var user = new FakeUserBuilder().Build();
        foreach (var role in roles)
            user.AddRole(role);
        
        await InsertAsync(user);
        return user;
    }}

    public static async Task<User> AddNewUser(params Role[] roles)
        => await AddNewUser(roles.ToList());"
            : null;

        var seedRootUser = hasAuth
            ? $@"
        
        // seed root user so tests won't always have user as super admin
        AddNewSuperAdmin().Wait();"
            : null;
        
        return @$"namespace {classNamespace};

using {contextClassPath.ClassNamespace};
using {apiClassPath.ClassNamespace};{authUsings}
using AutoBogus;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
 
[Collection(nameof(TestBase))]
public class TestBase : IDisposable
{{
    private static IServiceScopeFactory _scopeFactory;
    protected static HttpClient FactoryClient  {{ get; private set; }}

    public TestBase()
    {{
        var factory = new TestingWebApplicationFactory();
        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
        FactoryClient = factory.CreateClient(new WebApplicationFactoryClientOptions());

        AutoFaker.Configure(builder =>
        {{
            // configure global autobogus settings here
            builder.WithDateTimeKind(DateTimeKind.Utc)
                .WithRecursiveDepth(3)
                .WithTreeDepth(1)
                .WithRepeatCount(1);
        }});{seedRootUser}
    }}
    
    public void Dispose()
    {{
        FactoryClient.Dispose();
    }}

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {{
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetService<ISender>();
        return await mediator.Send(request);
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
        await action(scope.ServiceProvider);
    }}

    public static async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {{
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<{dbContextName}>();
        return await action(scope.ServiceProvider);
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
    }}{authHelperMethods}
}}";
    }
}
