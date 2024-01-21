namespace Craftsman.Builders.Features;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class CommandUpdateRecordBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public CommandUpdateRecordBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateCommand(string srcDirectory, Entity entity, string projectBaseName, bool isProtected, string permissionName)
    {
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{FileNames.UpdateEntityFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);
        var fileText = GetCommandFileText(classPath.ClassNamespace, entity, srcDirectory, projectBaseName, isProtected, permissionName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetCommandFileText(string classNamespace, Entity entity, string srcDirectory, string projectBaseName, bool isProtected, string permissionName)
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
        
        FeatureBuilderHelpers.GetPermissionValuesForHandlers(srcDirectory, 
            projectBaseName, 
            isProtected, 
            permissionName, 
            out string heimGuardCtor, 
            out string permissionCheck, 
            out string permissionsUsing);

        return @$"namespace {classNamespace};

using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {entityServicesClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using {modelClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};{permissionsUsing}
using Mappings;
using MediatR;

public static class {className}
{{
    public sealed record {updateCommandName}({primaryKeyPropType} {lowercasePrimaryKey}, {updateDto} {commandProp}) : IRequest;

    public sealed class Handler({repoInterface} {repoInterfaceProp}, IUnitOfWork unitOfWork{heimGuardCtor})
        : IRequestHandler<{updateCommandName}>
    {{
        public async Task Handle({updateCommandName} request, CancellationToken cancellationToken)
        {{{permissionCheck}
            var {updatedEntityProp} = await {repoInterfaceProp}.GetById(request.{lowercasePrimaryKey}, cancellationToken: cancellationToken);
            var {modelToUpdateVariableName} = request.{commandProp}.To{EntityModel.Update.GetClassName(entity.Name)}();
            {updatedEntityProp}.Update({modelToUpdateVariableName});

            {repoInterfaceProp}.Update({updatedEntityProp});
            await unitOfWork.CommitChanges(cancellationToken);
        }}
    }}
}}";
    }
}
