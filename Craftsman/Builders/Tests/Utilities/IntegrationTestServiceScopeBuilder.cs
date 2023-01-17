namespace Craftsman.Builders.Tests.Utilities;

using Helpers;
using Services;

public class IntegrationTestServiceScopeBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public IntegrationTestServiceScopeBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateBase(string solutionDirectory, string projectBaseName, string dbContextName, bool isProtected)
    {
        var classPath = ClassPathHelper.IntegrationTestProjectRootClassPath(solutionDirectory, $"{FileNames.TestingServiceScope()}.cs", projectBaseName);
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

using System.Threading.Tasks;
using Databases;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using {exceptionsClassPath.ClassNamespace};
using static {testFixtureName};{protectedUsings}

public class {FileNames.TestingServiceScope()} 
{{
    private readonly IServiceScope _scope;

    public {FileNames.TestingServiceScope()}()
    {{
        _scope = BaseScopeFactory.CreateScope();
    }}

    public TScopedService GetService<TScopedService>()
    {{
        var service = _scope.ServiceProvider.GetService<TScopedService>();
        return service;
    }}

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {{
        var mediator = _scope.ServiceProvider.GetService<ISender>();
        return await mediator.Send(request);
    }}

    public async Task<TEntity> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {{
        var context = _scope.ServiceProvider.GetService<{dbContextName}>();
        return await context.FindAsync<TEntity>(keyValues);
    }}

    public async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {{
        var context = _scope.ServiceProvider.GetService<{dbContextName}>();
        context.Add(entity);

        await context.SaveChangesAsync();
    }}

    public async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {{
        var dbContext = _scope.ServiceProvider.GetRequiredService<{dbContextName}>();

        try
        {{
            //await dbContext.BeginTransactionAsync();

            var result = await action(_scope.ServiceProvider);

            //await dbContext.CommitTransactionAsync();

            return result;
        }}
        catch (Exception)
        {{
            //dbContext.RollbackTransaction();
            throw;
        }}
    }}

    public Task<T> ExecuteDbContextAsync<T>(Func<{dbContextName}, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetService<{dbContextName}>()));
    
    public Task<int> InsertAsync<T>(params T[] entities) where T : class
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
}}";
    }
}
