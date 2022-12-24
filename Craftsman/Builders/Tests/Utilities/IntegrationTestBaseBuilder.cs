namespace Craftsman.Builders.Tests.Utilities;

using Helpers;
using Services;

public class IntegrationTestBaseBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public IntegrationTestBaseBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateBase(string solutionDirectory, string projectBaseName, string dbContextName, bool isProtected)
    {
        var classPath = ClassPathHelper.IntegrationTestProjectRootClassPath(solutionDirectory, "TestBase.cs", projectBaseName);
        var fileText = GetBaseText(classPath.ClassNamespace, dbContextName, isProtected, solutionDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetBaseText(string classNamespace, string dbContextName, bool isProtected, string solutionDirectory, string projectBaseName)
    {
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, projectBaseName);
        var testFixtureName = FileNames.GetIntegrationTestFixtureName();
        
        var protectedUsings = isProtected ? @$"{Environment.NewLine}using HeimGuard;
using Moq;" : "";
        var userDoesNotHavePermission = isProtected 
            ? $@"

    public void SetUserNotPermitted(string permission)
    {{
        var userPolicyHandler = GetService<IHeimGuardClient>();
        Mock.Get(userPolicyHandler)
            .Setup(x => x.MustHavePermission<ForbiddenAccessException>(permission))
            .ThrowsAsync(new ForbiddenAccessException());
        Mock.Get(userPolicyHandler)
            .Setup(x => x.HasPermissionAsync(permission))
            .ReturnsAsync(false);
    }}"
            : null;

        return @$"namespace {classNamespace};

using NUnit.Framework;
using System.Threading.Tasks;
using AutoBogus;
using Databases;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using {exceptionsClassPath.ClassNamespace};{protectedUsings}
using static {testFixtureName};

[Parallelizable]
public class TestBase
{{
    private static IServiceScopeFactory _scopeFactory;

    [SetUp]
    public Task TestSetUp()
    {{
        _scopeFactory = BaseScopeFactory;

        AutoFaker.Configure(builder =>
        {{
            // configure global autobogus settings here
            builder.WithDateTimeKind(DateTimeKind.Utc)
                .WithRecursiveDepth(3)
                .WithTreeDepth(1)
                .WithRepeatCount(1);
        }});        
        return Task.CompletedTask;
    }}

    public static TScopedService GetService<TScopedService>()
    {{
        var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetService<TScopedService>();
        return service;
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

    public static Task<T> ExecuteDbContextAsync<T>(Func<{dbContextName}, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<{dbContextName}>()));
    
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
    }}{userDoesNotHavePermission}
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
}}";
    }
}
