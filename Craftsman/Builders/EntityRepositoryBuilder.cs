namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class EntityRepositoryBuilder
{
    public class Command : IRequest<bool>
    {
        public readonly string DbContextName;
        public readonly string EntityName;
        public readonly string EntityPlural;

        public Command(string dbContextName, string entityName, string entityPlural)
        {
            DbContextName = dbContextName;
            EntityPlural = entityPlural;
            EntityName = entityName;
        }
    }

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command, bool>
    {
        public Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.EntityServicesClassPath(scaffoldingDirectoryStore.SrcDirectory, 
                $"{FileNames.EntityRepository(request.EntityName)}.cs", 
                request.EntityPlural, 
                scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace, request.EntityName, request.EntityPlural, request.DbContextName);
            utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private string GetFileText(string classNamespace, string entityName, string entityPlural, string dbContextName)
        {
            var entityClassPath = ClassPathHelper.EntityClassPath(scaffoldingDirectoryStore.SrcDirectory, 
                "", 
                entityPlural, 
                scaffoldingDirectoryStore.ProjectBaseName);
            var contextClassPath = ClassPathHelper.DbContextClassPath(scaffoldingDirectoryStore.SrcDirectory, 
                "", 
                scaffoldingDirectoryStore.ProjectBaseName);
            var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(scaffoldingDirectoryStore.SrcDirectory,
                "", 
                scaffoldingDirectoryStore.ProjectBaseName);

            var genericRepositoryInterface = FileNames.GenericRepositoryInterface();
            var genericRepoName = FileNames.GenericRepository();
            var repoInterface = FileNames.EntityRepositoryInterface(entityName);
            var repoName = FileNames.EntityRepository(entityName);
            
            return @$"namespace {classNamespace};

using {entityClassPath.ClassNamespace};
using {contextClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};

public interface {repoInterface} : {genericRepositoryInterface}<{entityName}>
{{
}}

public sealed class {repoName}({dbContextName} dbContext) : {genericRepoName}<{entityName}>(dbContext), {repoInterface}
{{
}}
";
        }
    }
}
