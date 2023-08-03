namespace Craftsman.Builders.Features;

using System;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class CommandAddListBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public CommandAddListBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateCommand(string srcDirectory, Entity entity, string projectBaseName, Feature feature, bool isProtected, string permissionName)
    {
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{feature.Name}.cs", entity.Plural, projectBaseName);
        var fileText = GetCommandFileText(classPath.ClassNamespace, entity, srcDirectory, feature, projectBaseName, isProtected, permissionName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetCommandFileText(string classNamespace, Entity entity, string srcDirectory, Feature feature, string projectBaseName, bool isProtected, string permissionName)
    {
        var className = feature.Name;
        var addCommandName = feature.Command;
        var readDto = FileNames.GetDtoName(entity.Name, Dto.Read);
        var readDtoAsList = $"List<{readDto}>";
        var createDto = FileNames.GetDtoName(entity.Name, Dto.Creation);
        createDto = $"IEnumerable<{createDto}>";
        var featurePropNameLowerFirst = feature.BatchPropertyName.LowercaseFirstLetter();

        var entityName = entity.Name;
        var entityNameLowercase = entity.Name.LowercaseFirstLetter();
        var entityNameLowercaseListVar = $"{entity.Name.LowercaseFirstLetter()}List";
        var primaryKeyPropName = Entity.PrimaryKeyProperty.Name;
        var commandProp = $"{entityName}ListToAdd";
        var newEntityProp = $"{entityNameLowercaseListVar}ListToAdd";
        var repoInterface = FileNames.EntityRepositoryInterface(entityName);
        var repoInterfaceProp = $"{entityName.LowercaseFirstLetter()}Repository";
        var repoInterfaceBatchFk = FileNames.EntityRepositoryInterface(feature.ParentEntity);
        var repoInterfacePropBatchFk = $"{feature.ParentEntity.LowercaseFirstLetter()}Repository";
        var modelToCreateVariableName = $"{entityName.LowercaseFirstLetter()}ToAdd";

        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var entityServicesClassPathBatchFk = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", feature.ParentEntityPlural, projectBaseName);
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "");
        var modelClassPath = ClassPathHelper.EntityModelClassPath(srcDirectory, entity.Name, entity.Plural, null, projectBaseName);
        
        FeatureBuilderHelpers.GetPermissionValuesForHandlers(srcDirectory, 
            projectBaseName, 
            isProtected, 
            permissionName, 
            out string heimGuardSetter, 
            out string heimGuardCtor, 
            out string permissionCheck, 
            out string permissionsUsing,
            out string heimGuardField);

        var batchFkCheck = !string.IsNullOrEmpty(feature.BatchPropertyName)
            ? @$"// throws error if parent doesn't exist 
            await _{repoInterfacePropBatchFk}.GetById(request.{feature.BatchPropertyName}, cancellationToken: cancellationToken);{Environment.NewLine}{Environment.NewLine}            "
            : "";
        var batchFkUsingRepo =  !string.IsNullOrEmpty(feature.BatchPropertyName)
            ? @$"{Environment.NewLine}using {entityServicesClassPathBatchFk.ClassNamespace};"
            : "";
        var batchFkDiReadonly =  !string.IsNullOrEmpty(feature.BatchPropertyName)
            ? @$"{Environment.NewLine}        private readonly {repoInterfaceBatchFk} _{repoInterfacePropBatchFk};"
            : "";
        var batchFkDiProp =  !string.IsNullOrEmpty(feature.BatchPropertyName)
            ? @$", {repoInterfaceBatchFk} {repoInterfacePropBatchFk}"
            : "";
        var batchFkDiPropSetter =  !string.IsNullOrEmpty(feature.BatchPropertyName)
            ? @$"{Environment.NewLine}            _{repoInterfacePropBatchFk} = {repoInterfacePropBatchFk};"
            : "";

        return @$"namespace {classNamespace};

using {entityServicesClassPath.ClassNamespace};{batchFkUsingRepo}
using {servicesClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {modelClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};{permissionsUsing}
using Mappings;
using MediatR;

public static class {className}
{{
    public sealed record {addCommandName}({createDto} {commandProp}, {feature.BatchPropertyType} {feature.BatchPropertyName}) : IRequest<{readDtoAsList}>;

    public sealed class Handler : IRequestHandler<{addCommandName}, {readDtoAsList}>
    {{
        private readonly {repoInterface} _{repoInterfaceProp};{batchFkDiReadonly}
        private readonly IUnitOfWork _unitOfWork;{heimGuardField}

        public Handler({repoInterface} {repoInterfaceProp}, IUnitOfWork unitOfWork{batchFkDiProp}{heimGuardCtor})
        {{
            _{repoInterfaceProp} = {repoInterfaceProp};
            _unitOfWork = unitOfWork;{batchFkDiPropSetter}{heimGuardSetter}
        }}

        public async Task<{readDtoAsList}> Handle({addCommandName} request, CancellationToken cancellationToken)
        {{{permissionCheck}
            {batchFkCheck}var {entityNameLowercaseListVar}ToAdd = request.{commandProp}
                .Select({entity.Lambda} => {{ {entity.Lambda}.{feature.BatchPropertyName} = request.{feature.BatchPropertyName}; return {entity.Lambda}; }})
                .ToList();
            var {entityNameLowercaseListVar} = new List<{entityName}>();
            foreach (var {entityNameLowercase} in {entityNameLowercaseListVar}ToAdd)
            {{
                var {entityNameLowercase}ToAdd = {entityNameLowercase}.To{EntityModel.Creation.GetClassName(entity.Name)}();
                {entityNameLowercaseListVar}.Add({entityName}.Create({entityNameLowercase}ToAdd));
            }}

            // if you have large datasets to add in bulk and have performance concerns, there 
            // are additional methods that could be leveraged in your repository instead (e.g. SqlBulkCopy)
            // https://timdeschryver.dev/blog/faster-sql-bulk-inserts-with-csharp#table-valued-parameter 
            await _{repoInterfaceProp}.AddRange({entityNameLowercaseListVar}, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            return {entityNameLowercaseListVar}
                .Select({entity.Lambda} => {entity.Lambda}.To{readDto}())
                .ToList();
        }}
    }}
}}";
    }
}
