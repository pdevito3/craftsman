namespace Craftsman.Builders.Features;

using Domain;
using Domain.Enums;
using Helpers;
using MediatR;
using Services;

public static class CommandAddRecordBuilder
{
    public class CommandAddRecordBuilderCommand(Entity entity, bool isProtected, string permission, string dbContextName) : IRequest
    {
        public Entity Entity { get; set; } = entity;
        public bool IsProtected { get; set; } = isProtected;
        public string Permission { get; set; } = permission;
        public string DbContextName { get; set; } = dbContextName;
    }

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<CommandAddRecordBuilderCommand>
    {
        public Task Handle(CommandAddRecordBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.FeaturesClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{FileNames.AddEntityFeatureClassName(request.Entity.Name)}.cs", request.Entity.Plural, scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetCommandFileText(classPath.ClassNamespace, request.Entity, scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.ProjectBaseName, request.IsProtected, request.Permission, request.DbContextName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }

        private static string GetCommandFileText(string classNamespace, Entity entity, string srcDirectory, string projectBaseName, bool isProtected, string permissionName, string dbContextName)
        {
            var className = FileNames.AddEntityFeatureClassName(entity.Name);
            var addCommandName = FileNames.CommandAddName();
            var readDto = FileNames.GetDtoName(entity.Name, Dto.Read);
            var createDto = FileNames.GetDtoName(entity.Name, Dto.Creation);
            var creationModelName = EntityModel.Creation.GetClassName(entity.Name);

            var entityName = entity.Name;
            var entityNameLowercase = entity.Name.LowercaseFirstLetter();
            var commandProp = $"{entityName}ToAdd";
            var newEntityProp = $"{entityNameLowercase}ToAdd";
            var repoInterface = FileNames.EntityRepositoryInterface(entityName);
            var repoInterfaceProp = $"{entityName.LowercaseFirstLetter()}Repository";
            var modelToCreateVariableName = $"{entityName.LowercaseFirstLetter()}ToAdd";

            var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
            var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", entity.Plural, projectBaseName);
            var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
            var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);
            var modelClassPath = ClassPathHelper.EntityModelClassPath(srcDirectory, entity.Name, entity.Plural, null, projectBaseName);
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
                     using {{entityClassPath.ClassNamespace}};
                     using {{dtoClassPath.ClassNamespace}};
                     using {{modelClassPath.ClassNamespace}};
                     using {{servicesClassPath.ClassNamespace}};
                     using {{exceptionsClassPath.ClassNamespace}};{{permissionsUsing}}
                     using Mappings;
                     using MediatR;

                     public static class {{className}}
                     {
                         public sealed record {{addCommandName}}({{createDto}} {{commandProp}}) : IRequest<{{readDto}}>;

                         public sealed class Handler({{dbContextName}} dbContext{{heimGuardCtor}})
                             : IRequestHandler<{{addCommandName}}, {{readDto}}>
                         {
                             public async Task<{{readDto}}> Handle({{addCommandName}} request, CancellationToken cancellationToken)
                             {{{permissionCheck}}
                                 var {{modelToCreateVariableName}} = request.{{commandProp}}.To{{EntityModel.Creation.GetClassName(entity.Name)}}();
                                 var {{entityNameLowercase}} = {{entityName}}.Create({{modelToCreateVariableName}});

                                 await dbContext.{{entity.Plural}}.AddAsync({{entityNameLowercase}}, cancellationToken);
                                 await dbContext.SaveChangesAsync(cancellationToken);

                                 return {{entityNameLowercase}}.To{{readDto}}();
                             }
                         }
                     }
                     """;
        }
    }
}
