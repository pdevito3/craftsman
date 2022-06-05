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

    public void CreateCommand(string solutionDirectory, string srcDirectory, Entity entity, string contextName, string projectBaseName, Feature feature)
    {
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{feature.Name}.cs", entity.Plural, projectBaseName);
        var fileText = GetCommandFileText(classPath.ClassNamespace, entity, contextName, solutionDirectory, srcDirectory, feature, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetCommandFileText(string classNamespace, Entity entity, string contextName, string solutionDirectory, string srcDirectory, Feature feature, string projectBaseName)
    {
        var className = feature.Name;
        var addCommandName = feature.Command;
        var readDto = FileNames.GetDtoName(entity.Name, Dto.Read);
        readDto = $"IEnumerable<{readDto}>";
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

        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var entityServicesClassPathBatchFk = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", feature.ParentEntityPlural, projectBaseName);
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);

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
using AutoMapper;
using MediatR;

public static class {className}
{{
    public class {addCommandName} : IRequest<{readDto}>
    {{
        public readonly {createDto} {commandProp};
        public readonly {feature.BatchPropertyType} {feature.BatchPropertyName};

        public {addCommandName}({createDto} {newEntityProp}, {feature.BatchPropertyType} {featurePropNameLowerFirst})
        {{
            {commandProp} = {newEntityProp};
            {feature.BatchPropertyName} = {featurePropNameLowerFirst};
        }}
    }}

    public class Handler : IRequestHandler<{addCommandName}, {readDto}>
    {{
        private readonly {repoInterface} _{repoInterfaceProp};{batchFkDiReadonly}
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public Handler({repoInterface} {repoInterfaceProp}, IUnitOfWork unitOfWork, IMapper mapper{batchFkDiProp})
        {{
            _mapper = mapper;
            _{repoInterfaceProp} = {repoInterfaceProp};{batchFkDiPropSetter}
            _unitOfWork = unitOfWork;
        }}

        public async Task<{readDto}> Handle({addCommandName} request, CancellationToken cancellationToken)
        {{
            {batchFkCheck}var {entityNameLowercaseListVar}ToAdd = request.{commandProp}
                .Select({entity.Lambda} => {{ {entity.Lambda}.{feature.BatchPropertyName} = request.{feature.BatchPropertyName}; return {entity.Lambda}; }})
                .ToList();
            var {entityNameLowercaseListVar} = new List<{entityName}>();
            {entityNameLowercaseListVar}ToAdd.ForEach({entityNameLowercase} => {entityNameLowercaseListVar}.Add({entityName}.Create({entityNameLowercase})));
            
            // if you have large datasets to add in bulk and have performance concerns, there 
            // are additional methods that could be leveraged in your repository instead (e.g. SqlBulkCopy)
            // https://timdeschryver.dev/blog/faster-sql-bulk-inserts-with-csharp#table-valued-parameter 
            await _{repoInterfaceProp}.AddRange({entityNameLowercaseListVar}, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            var result = _{repoInterfaceProp}
                .Query()
                .Where({entity.Lambda} => {entityNameLowercaseListVar}.Select(listItem => listItem.{primaryKeyPropName}).Contains({entity.Lambda}.{primaryKeyPropName}));
            return _mapper.Map<{readDto}>(result);
        }}
    }}
}}";
    }
}
