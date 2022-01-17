namespace Craftsman.Builders.Features
{
    using System;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class CommandAddListBuilder
    {
        public static void CreateCommand(string solutionDirectory, string srcDirectory, Entity entity, string contextName, string projectBaseName, Feature feature, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{feature.Name}.cs", entity.Plural, projectBaseName);
            var fileText = GetCommandFileText(classPath.ClassNamespace, entity, contextName, solutionDirectory, srcDirectory, feature, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static string GetCommandFileText(string classNamespace, Entity entity, string contextName, string solutionDirectory, string srcDirectory, Feature feature, string projectBaseName)
        {
            var className = feature.Name;
            var addCommandName = feature.Command;
            var readDto = Utilities.GetDtoName(entity.Name, Dto.Read);
            readDto = $"IEnumerable<{readDto}>";
            var createDto = Utilities.GetDtoName(entity.Name, Dto.Creation);
            createDto = $"IEnumerable<{createDto}>";
            var featurePropNameLowerFirst = feature.BatchPropertyName.LowercaseFirstLetter();

            var entityName = entity.Name;
            var entityNameLowercase = entity.Name.LowercaseFirstLetter();
            var entityNameLowercaseListVar = $"{entity.Name.LowercaseFirstLetter()}List";
            var primaryKeyPropName = Entity.PrimaryKeyProperty.Name;
            var commandProp = $"{entityName}ListToAdd";
            var newEntityProp = $"{entityNameLowercaseListVar}ListToAdd";

            var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name);
            var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "");
            var contextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
            var validatorsClassPath = ClassPathHelper.ValidationClassPath(srcDirectory, "", entity.Plural, projectBaseName);

            var batchFkCheck = !string.IsNullOrEmpty(feature.BatchPropertyDbSetName)
                ? @$"var fkEntity = await _db.{feature.BatchPropertyDbSetName}.Where(x => x.Id == request.{feature.BatchPropertyName}).FirstOrDefaultAsync(cancellationToken);
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
            {entityNameLowercaseListVar}ToAdd.ForEach({entityNameLowercase} => {entityNameLowercaseListVar}.Add(Ingredient.Create({entityNameLowercase})));
            
            _db.{entity.Plural}.AddRange({entityNameLowercaseListVar});

            await _db.SaveChangesAsync(cancellationToken);

            var result = _db.{entity.Plural}.Where({entity.Lambda} => {entityNameLowercaseListVar}.Select({entity.Lambda} => {entity.Lambda}.{primaryKeyPropName}).Contains({entity.Lambda}.{primaryKeyPropName}));
            return _mapper.Map<{readDto}>(result);
        }}
    }}
}}";
        }
    }
}