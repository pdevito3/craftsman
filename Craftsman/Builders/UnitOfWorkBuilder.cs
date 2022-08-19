namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class UnitOfWorkBuilder
{
    public class UnitOfWorkBuilderCommand : IRequest<bool>
    {
        public readonly string DbContextName;

        public UnitOfWorkBuilderCommand(string dbContextName)
        {
            DbContextName = dbContextName;
        }
    }

    public class Handler : IRequestHandler<UnitOfWorkBuilderCommand, bool>
    {
        private readonly ICraftsmanUtilities _utilities;
        private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

        public Handler(ICraftsmanUtilities utilities,
            IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        {
            _utilities = utilities;
            _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        }

        public Task<bool> Handle(UnitOfWorkBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.WebApiServicesClassPath(_scaffoldingDirectoryStore.SrcDirectory, "UnitOfWork.cs", _scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace, request.DbContextName);
            _utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private string GetFileText(string classNamespace, string dbContextName)
        {
            var boundaryServiceName = FileNames.BoundaryServiceInterface(_scaffoldingDirectoryStore.ProjectBaseName);
            var contextClassPath = ClassPathHelper.DbContextClassPath(_scaffoldingDirectoryStore.SrcDirectory, "", _scaffoldingDirectoryStore.ProjectBaseName);
            
            return @$"namespace {classNamespace};

using {contextClassPath.ClassNamespace};

public interface IUnitOfWork : {boundaryServiceName}
{{
    Task<int> CommitChanges(CancellationToken cancellationToken = default);
}}

public class UnitOfWork : IUnitOfWork
{{
    private readonly {dbContextName} _dbContext;

    public UnitOfWork({dbContextName} dbContext)
    {{
        _dbContext = dbContext;
    }}

    public async Task<int> CommitChanges(CancellationToken cancellationToken = default)
    {{
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }}
}}
";
        }
    }
}
