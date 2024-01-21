namespace Craftsman.Builders.Features;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class CommandAddRecordBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public CommandAddRecordBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateCommand(string srcDirectory, Entity entity, string projectBaseName, bool isProtected, string permission)
    {
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{FileNames.AddEntityFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);
        var fileText = GetCommandFileText(classPath.ClassNamespace, entity, srcDirectory, projectBaseName, isProtected, permission);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetCommandFileText(string classNamespace, Entity entity, string srcDirectory, string projectBaseName, bool isProtected, string permissionName)
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
        
        FeatureBuilderHelpers.GetPermissionValuesForHandlers(srcDirectory, 
            projectBaseName, 
            isProtected, 
            permissionName, 
            out string heimGuardCtor, 
            out string permissionCheck, 
            out string permissionsUsing);

        return @$"namespace {classNamespace};

using {entityServicesClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {modelClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};{permissionsUsing}
using Mappings;
using MediatR;

public static class {className}
{{
    public sealed record {addCommandName}({createDto} {commandProp}) : IRequest<{readDto}>;

    public sealed class Handler({repoInterface} {repoInterfaceProp}, IUnitOfWork unitOfWork{heimGuardCtor})
        : IRequestHandler<{addCommandName}, {readDto}>
    {{
        public async Task<{readDto}> Handle({addCommandName} request, CancellationToken cancellationToken)
        {{{permissionCheck}
            var {modelToCreateVariableName} = request.{commandProp}.To{EntityModel.Creation.GetClassName(entity.Name)}();
            var {entityNameLowercase} = {entityName}.Create({modelToCreateVariableName});

            await {repoInterfaceProp}.Add({entityNameLowercase}, cancellationToken);
            await unitOfWork.CommitChanges(cancellationToken);

            return {entityNameLowercase}.To{readDto}();
        }}
    }}
}}";
    }
}
