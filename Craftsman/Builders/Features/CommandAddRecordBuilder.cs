namespace Craftsman.Builders.Features
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.Text;

    public class CommandAddRecordBuilder
    {
        public static void CreateCommand(string solutionDirectory, Entity entity, string contextName, string projectBaseName)
        {
            var classPath = ClassPathHelper.FeaturesClassPath(solutionDirectory, $"{Utilities.AddEntityFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = "";
                data = GetCommandFileText(classPath.ClassNamespace, entity, contextName, solutionDirectory, projectBaseName);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static string GetCommandFileText(string classNamespace, Entity entity, string contextName, string solutionDirectory, string projectBaseName)
        {
            var className = Utilities.AddEntityFeatureClassName(entity.Name);
            var addCommandName = Utilities.CommandAddName(entity.Name);
            var readDto = Utilities.GetDtoName(entity.Name, Dto.Read);
            var createDto = Utilities.GetDtoName(entity.Name, Dto.Creation);
            var manipulationValidator = Utilities.ValidatorNameGenerator(entity.Name, Validator.Manipulation);

            var entityName = entity.Name;
            var entityNameLowercase = entity.Name.LowercaseFirstLetter();
            var primaryKeyPropName = entity.PrimaryKeyProperty.Name;
            var commandProp = $"{entityName}ToAdd";
            var newEntityProp = $"{entityNameLowercase}ToAdd";

            var entityClassPath = ClassPathHelper.EntityClassPath(solutionDirectory, "", entity.Plural, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, "", projectBaseName);
            var contextClassPath = ClassPathHelper.DbContextClassPath(solutionDirectory, "", projectBaseName);
            var validatorsClassPath = ClassPathHelper.ValidationClassPath(solutionDirectory, "", entity.Plural, projectBaseName);

            return @$"namespace {classNamespace}
{{
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

    public static class {className}
    {{
        public class {addCommandName} : IRequest<{readDto}>
        {{
            public {createDto} {commandProp} {{ get; set; }}

            public {addCommandName}({createDto} {newEntityProp})
            {{
                {commandProp} = {newEntityProp};
            }}
        }}

        public class CustomCreate{entityName}Validation : {manipulationValidator}<{createDto}>
        {{
            public CustomCreate{entityName}Validation()
            {{
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
                var {entityNameLowercase} = _mapper.Map<{entityName}> (request.{commandProp});
                _db.{entity.Plural}.Add({entityNameLowercase});

                await _db.SaveChangesAsync();

                return await _db.{entity.Plural}
                    .ProjectTo<{readDto}>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.{primaryKeyPropName} == {entityNameLowercase}.{primaryKeyPropName});
            }}
        }}
    }}
}}";
        }
    }
}