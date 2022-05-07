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

    public void CreateBase(string solutionDirectory, string projectBaseName, string dbContextName)
    {
        var classPath = ClassPathHelper.FunctionalTestProjectRootClassPath(solutionDirectory, "TestBase.cs", projectBaseName);
        var fileText = GetBaseText(classPath.ClassNamespace, solutionDirectory, projectBaseName, dbContextName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetBaseText(string classNamespace, string solutionDirectory, string projectBaseName, string dbContextName)
    {
        var contextClassPath = ClassPathHelper.DbContextClassPath(solutionDirectory, "", projectBaseName);
        var apiClassPath = ClassPathHelper.WebApiProjectRootClassPath(solutionDirectory, "", projectBaseName);

        return @$"namespace {classNamespace};

using {contextClassPath.ClassNamespace};
using {apiClassPath.ClassNamespace};
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Threading.Tasks;

public class TestBase
{{
    public static IServiceScopeFactory _scopeFactory;
    public static WebApplicationFactory<Program> _factory;
    public static HttpClient _client;

    [SetUp]
    public void TestSetUp()
    {{
        _factory = new {FileNames.GetWebHostFactoryName()}();
        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions());
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
}}";
    }
}
