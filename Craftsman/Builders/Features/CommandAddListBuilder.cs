namespace Craftsman.Builders.Features
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class CommandAddListBuilder
    {
        public static void CreateCommand(string srcDirectory, Entity entity, string contextName, string projectBaseName, Feature feature, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{feature.Command}Tests.cs", entity.Plural, projectBaseName);
            var fileText = GetCommandFileText(classPath.ClassNamespace, entity, contextName, srcDirectory, feature, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static string GetCommandFileText(string classNamespace, Entity entity, string contextName, string solutionDirectory, Feature feature, string projectBaseName)
        {
            var className = feature.Name;
            var addCommandName = feature.Command;
            var readDto = Utilities.GetDtoName(entity.Name, Dto.Read);
            readDto = $"IEnumerable<{readDto}>";
            var createDto = Utilities.GetDtoName(entity.Name, Dto.Creation);
            createDto = $"IEnumerable<{createDto}>";
            var featurePropNameLowerFirst = feature.BatchPropertyName.LowercaseFirstLetter();

            var entityName = entity.Name;
            var entityNameLowercase = $"{entity.Name.LowercaseFirstLetter()}List";
            var primaryKeyPropName = Entity.PrimaryKeyProperty.Name;
            var commandProp = $"{entityName}ListToAdd";
            var newEntityProp = $"{entityNameLowercase}ListToAdd";

            var entityClassPath = ClassPathHelper.EntityClassPath(solutionDirectory, "", entity.Plural, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, "", projectBaseName);
            var contextClassPath = ClassPathHelper.DbContextClassPath(solutionDirectory, "", projectBaseName);
            var validatorsClassPath = ClassPathHelper.ValidationClassPath(solutionDirectory, "", entity.Plural, projectBaseName);

            var batchFkCheck = !string.IsNullOrEmpty(feature.BatchPropertyDbSetName)
                ? @$"var fkEntity = await _db.{feature.BatchPropertyDbSetName}.Where(x => x.Id == request.{feature.BatchPropertyName}).FirstOrDefaultAsync(cancellationToken);
                 
                if (fkEntity == null)
                    throw new KeyNotFoundException($""No {feature.BatchPropertyName} found with an id of '{{request.{feature.BatchPropertyName}}}'"");"
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
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

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
            var {entityNameLowercase} = _mapper.Map<IEnumerable<{entityName}>> (request.{commandProp});
            {entityNameLowercase} = {entityNameLowercase}.ToList().Select({entity.Lambda} => {{ {entity.Lambda}.{feature.BatchPropertyName} = request.{feature.BatchPropertyName}; return {entity.Lambda}; }});
            {batchFkCheck}
            
            _db.{entity.Plural}.AddRange({entityNameLowercase});

            await _db.SaveChangesAsync();

            var result = _db.{entity.Plural}.Where({entity.Lambda} => {entityNameLowercase}.Select({entity.Lambda} => {entity.Lambda}.{primaryKeyPropName}).Contains({entity.Lambda}.{primaryKeyPropName}));
            return _mapper.Map<{readDto}>(result);
        }}
    }}
}}";
        }
    }
}