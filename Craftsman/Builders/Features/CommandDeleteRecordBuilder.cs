namespace Craftsman.Builders.Features;

using Domain;
using Helpers;
using MediatR;
using Services;

public static class CommandDeleteRecordBuilder
{
    public class CommandDeleteRecordBuilderCommand(Entity entity, bool isProtected, string permissionName, string dbContextName) : IRequest<bool>
    {
        public Entity Entity { get; set; } = entity;
        public bool IsProtected { get; set; } = isProtected;
        public string PermissionName { get; set; } = permissionName;
        public string DbContextName { get; set; } = dbContextName;
    }

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<CommandDeleteRecordBuilderCommand, bool>
    {
        public Task<bool> Handle(CommandDeleteRecordBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.FeaturesClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{FileNames.DeleteEntityFeatureClassName(request.Entity.Name)}.cs", request.Entity.Plural, scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetCommandFileText(classPath.ClassNamespace, request.Entity, scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.ProjectBaseName, request.IsProtected, request.PermissionName, request.DbContextName);
            utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private static string GetCommandFileText(string classNamespace, Entity entity, string srcDirectory, string projectBaseName, bool isProtected, string permissionName, string dbContextName)
        {
            var className = FileNames.DeleteEntityFeatureClassName(entity.Name);
            var deleteCommandName = FileNames.CommandDeleteName();

            var primaryKeyPropType = Entity.PrimaryKeyProperty.Type;
            var lowercasePrimaryKey = $"{entity.Name}Id";
            var primaryKeyPropNameLowercase = Entity.PrimaryKeyProperty.Name.LowercaseFirstLetter();
            var repoInterface = FileNames.EntityRepositoryInterface(entity.Name);
            var repoInterfaceProp = $"{entity.Name.LowercaseFirstLetter()}Repository";

            var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", entity.Plural, projectBaseName);
            var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
            var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);
            var dbContextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
            
            FeatureBuilderHelpers.GetPermissionValuesForHandlers(srcDirectory, 
                projectBaseName, 
                isProtected, 
                permissionName, 
                out string heimGuardCtor, 
                out string permissionCheck, 
                out string permissionsUsing);

            // lang=csharp
            return $$"""
                     namespace {{classNamespace}};

                     using {{dbContextClassPath.ClassNamespace}};
                     using {{servicesClassPath.ClassNamespace}};
                     using {{exceptionsClassPath.ClassNamespace}};{{permissionsUsing}}
                     using MediatR;

                     public static class {{className}}
                     {
                         public sealed record {{deleteCommandName}}({{primaryKeyPropType}} {{lowercasePrimaryKey}}) : IRequest;

                         public sealed class Handler({{dbContextName}} dbContext{{heimGuardCtor}})
                             : IRequestHandler<{{deleteCommandName}}>
                         {
                             public async Task Handle({{deleteCommandName}} request, CancellationToken cancellationToken)
                             {{{permissionCheck}}
                                 var recordToDelete = await dbContext.{{entity.Plural}}
                                     .GetById(request.{{lowercasePrimaryKey}}, cancellationToken: cancellationToken);
                                 dbContext.Remove(recordToDelete);
                                 await dbContext.SaveChangesAsync(cancellationToken);
                             }
                         }
                     }
                     """;
        }
    }
}
