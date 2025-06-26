namespace Craftsman.Builders.Features;

using Domain;
using Domain.Enums;
using Helpers;
using Services;
using MediatR;

public static class CommandUpdateRecordBuilder
{
    public class Command(Entity entity, bool isProtected, string permissionName, string dbContextName) : IRequest<bool>
    {
        public Entity Entity { get; set; } = entity;
        public bool IsProtected { get; set; } = isProtected;
        public string PermissionName { get; set; } = permissionName;
        public string DbContextName { get; set; } = dbContextName;
    }

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command, bool>
    {
        public Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.FeaturesClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{FileNames.UpdateEntityFeatureClassName(request.Entity.Name)}.cs", request.Entity.Plural, scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace, request.Entity, scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.ProjectBaseName, request.IsProtected, request.PermissionName, request.DbContextName);
            utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private static string GetFileText(string classNamespace, Entity entity, string srcDirectory, string projectBaseName, bool isProtected, string permissionName, string dbContextName)
    {
        var className = FileNames.UpdateEntityFeatureClassName(entity.Name);
        var updateCommandName = FileNames.CommandUpdateName();
        var updateDto = FileNames.GetDtoName(entity.Name, Dto.Update);

        var primaryKeyPropType = Entity.PrimaryKeyProperty.Type;
        var lowercasePrimaryKey = $"{entity.Name}Id";
        var entityNameLowercase = entity.Name.LowercaseFirstLetter();
        var commandProp = $"Updated{entity.Name}Data";
        var newEntityDataProp = $"updated{entity.Name}Data";
        var updatedEntityProp = $"{entityNameLowercase}ToUpdate";
        var modelToUpdateVariableName = $"{entity.Name.LowercaseFirstLetter()}ToAdd";
        var repoInterface = FileNames.EntityRepositoryInterface(entity.Name);
        var repoInterfaceProp = $"{entity.Name.LowercaseFirstLetter()}Repository";

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
            return $@"namespace {classNamespace};

using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {dbContextClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using {modelClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};{permissionsUsing}
using Mappings;
using MediatR;

public static class {className}
{{
    public sealed record {updateCommandName}({primaryKeyPropType} {lowercasePrimaryKey}, {updateDto} {commandProp}) : IRequest;

    public sealed class Handler({dbContextName} dbContext{heimGuardCtor})
        : IRequestHandler<{updateCommandName}>
    {{
        public async Task Handle({updateCommandName} request, CancellationToken cancellationToken)
        {{{permissionCheck}
            var {updatedEntityProp} = await dbContext.{entity.Plural}.GetById(request.{lowercasePrimaryKey}, cancellationToken: cancellationToken);
            var {modelToUpdateVariableName} = request.{commandProp}.To{EntityModel.Update.GetClassName(entity.Name)}();
            {updatedEntityProp}.Update({modelToUpdateVariableName});

            await dbContext.SaveChangesAsync(cancellationToken);
        }}
    }}
}}";
        }
    }
}
