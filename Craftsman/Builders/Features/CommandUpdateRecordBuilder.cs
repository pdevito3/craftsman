namespace Craftsman.Builders.Features
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.Text;

    public class CommandUpdateRecordBuilder
    {
        public static void CreateCommand(string solutionDirectory, Entity entity, string contextName, string projectBaseName)
        {
            var classPath = ClassPathHelper.FeaturesClassPath(solutionDirectory, $"{Utilities.UpdateEntityFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);

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
            var className = Utilities.UpdateEntityFeatureClassName(entity.Name);
            var updateCommandName = Utilities.CommandUpdateName(entity.Name);
            var updateDto = Utilities.GetDtoName(entity.Name, Dto.Update);
            var manipulationValidator = Utilities.ValidatorNameGenerator(entity.Name, Validator.Manipulation);

            var primaryKeyPropType = entity.PrimaryKeyProperty.Type;
            var primaryKeyPropName = entity.PrimaryKeyProperty.Name;
            var entityNameLowercase = entity.Name.LowercaseFirstLetter();
            var commandProp = $"{entity.Name}ToUpdate";
            var newEntitDataProp = $"New{entity.Name}Data";
            var updatedEntityProp = $"{entityNameLowercase}ToUpdate";

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
        public class {updateCommandName} : IRequest<bool>
        {{
            public {primaryKeyPropType} {primaryKeyPropName} {{ get; set; }}
            public {updateDto} {commandProp} {{ get; set; }}

            public {updateCommandName}({primaryKeyPropType} {entityNameLowercase}, {updateDto} {newEntitDataProp})
            {{
                {primaryKeyPropName} = {entityNameLowercase};
                {commandProp} = {newEntitDataProp};
            }}
        }}

        public class CustomUpdate{entity.Name}Validation : {manipulationValidator}<{updateDto}>
        {{
            public CustomUpdate{entity.Name}Validation()
            {{
            }}
        }}

        public class Handler : IRequestHandler<{updateCommandName}, bool>
        {{
            private readonly {contextName} _db;
            private readonly IMapper _mapper;

            public Handler({contextName} db, IMapper mapper)
            {{
                _mapper = mapper;
                _db = db;
            }}

            public async Task<bool> Handle({updateCommandName} request, CancellationToken cancellationToken)
            {{
                var {updatedEntityProp} = await _db.{entity.Plural}
                    .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.{primaryKeyPropName} == request.{primaryKeyPropName});

                if ({updatedEntityProp} == null)
                {{
                    // TODO log error
                    throw new KeyNotFoundException();
                }}

                _mapper.Map(request.{commandProp}, {updatedEntityProp});

                await _db.SaveChangesAsync();

                return true;
            }}
        }}
    }}
}}";
        }
    }
}