namespace Craftsman.Builders.Features
{
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

            var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "");
            var contextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
            var validatorsClassPath = ClassPathHelper.ValidationClassPath(srcDirectory, "", entity.Plural, projectBaseName);

            var batchFkCheck = !string.IsNullOrEmpty(feature.BatchPropertyDbSetName)
                ? @$"var fkEntity = await _db.{feature.BatchPropertyDbSetName}.FirstOrDefaultAsync(x => x.Id == request.{feature.BatchPropertyName}, cancellationToken);
            if (fkEntity == null)
                throw new NotFoundException($""No {feature.BatchPropertyName} found with an id of '{{request.{feature.BatchPropertyName}}}'"");{Environment.NewLine}{Environment.NewLine}"
                : "";

            return @$"namespace {classNamespace};

using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};
using {contextClassPath.ClassNamespace};
using {validatorsClassPath.ClassNamespace};
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public static class {className}
{{
    public class {addCommandName} : IRequest<{readDto}>
    {{
        public {createDto} {commandProp} {{ get; set; }}
        public {feature.BatchPropertyType} {feature.BatchPropertyName} {{ get; set; }}

        public {addCommandName}({createDto} {newEntityProp}, {feature.BatchPropertyType} {featurePropNameLowerFirst})
        {{
            {commandProp} = {newEntityProp};
            {feature.BatchPropertyName} = {featurePropNameLowerFirst};
        }}
    }}

    public class Handler : IRequestHandler<{addCommandName}, {readDto}>
    {{
        private readonly {contextName} _db;
        private readonly IMapper _mapper;

        public Handler({contextName} db, IMapper mapper)
        {{
            _mapper = mapper;
            _db = db;
        }}

        public async Task<{readDto}> Handle({addCommandName} request, CancellationToken cancellationToken)
        {{
            {batchFkCheck}var {entityNameLowercaseListVar}ToAdd = request.{commandProp}
                .Select({entity.Lambda} => {{ {entity.Lambda}.{feature.BatchPropertyName} = request.{feature.BatchPropertyName}; return {entity.Lambda}; }})
                .ToList();
            var {entityNameLowercaseListVar} = new List<{entityName}>();
            {entityNameLowercaseListVar}ToAdd.ForEach({entityNameLowercase} => {entityNameLowercaseListVar}.Add({entityName}.Create({entityNameLowercase})));
            
            _db.{entity.Plural}.AddRangeAsync({entityNameLowercaseListVar}, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);

            var result = _db.{entity.Plural}.Where({entity.Lambda} => {entityNameLowercaseListVar}.Select({entity.Lambda}l => {entity.Lambda}l.{primaryKeyPropName}).Contains({entity.Lambda}.{primaryKeyPropName}));
            return _mapper.Map<{readDto}>(result);
        }}
    }}
}}";
        }
    }
}