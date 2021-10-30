namespace Craftsman.Builders.Features
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.Text;

    public class CommandPatchRecordBuilder
    {
        public static void CreateCommand(string srcDirectory, Entity entity, string contextName, string projectBaseName)
        {
            var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{Utilities.PatchEntityFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using FileStream fs = File.Create(classPath.FullClassPath);
            var data = "";
            data = GetCommandFileText(classPath.ClassNamespace, entity, contextName, srcDirectory, projectBaseName);
            fs.Write(Encoding.UTF8.GetBytes(data));
        }

        public static string GetCommandFileText(string classNamespace, Entity entity, string contextName, string srcDirectory, string projectBaseName)
        {
            var className = Utilities.PatchEntityFeatureClassName(entity.Name);
            var patchCommandName = Utilities.CommandPatchName(entity.Name);
            var updateDto = Utilities.GetDtoName(entity.Name, Dto.Update);
            var manipulationValidator = Utilities.ValidatorNameGenerator(entity.Name, Validator.Manipulation);

            var primaryKeyPropType = Entity.PrimaryKeyProperty.Type;
            var primaryKeyPropName = Entity.PrimaryKeyProperty.Name;
            var entityNameLowercase = entity.Name.LowercaseFirstLetter();
            var updatedEntityProp = $"{entityNameLowercase}ToUpdate";
            var patchedEntityProp = $"{entityNameLowercase}ToPatch";

            var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Name, projectBaseName);
            var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);
            var contextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
            var validatorsClassPath = ClassPathHelper.ValidationClassPath(srcDirectory, "", entity.Plural, projectBaseName);

            return @$"namespace {classNamespace};

using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};
using {contextClassPath.ClassNamespace};
using {validatorsClassPath.ClassNamespace};
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

public static class {className}
{{
    public class {patchCommandName} : IRequest<bool>
    {{
        public {primaryKeyPropType} {primaryKeyPropName} {{ get; set; }}
        public JsonPatchDocument<{updateDto}> PatchDoc {{ get; set; }}

        public {patchCommandName}({primaryKeyPropType} {entityNameLowercase}, JsonPatchDocument<{updateDto}> patchDoc)
        {{
            {primaryKeyPropName} = {entityNameLowercase};
            PatchDoc = patchDoc;
        }}
    }}

    public class Handler : IRequestHandler<{patchCommandName}, bool>
    {{
        private readonly {contextName} _db;
        private readonly IMapper _mapper;

        public Handler({contextName} db, IMapper mapper)
        {{
            _mapper = mapper;
            _db = db;
        }}

        public async Task<bool> Handle({patchCommandName} request, CancellationToken cancellationToken)
        {{
            if (request.PatchDoc == null)
                throw new ApiException(""Invalid patch document."");

            var {updatedEntityProp} = await _db.{entity.Plural}
                .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.{primaryKeyPropName} == request.{primaryKeyPropName});

            if ({updatedEntityProp} == null)
                throw new KeyNotFoundException();

            var {patchedEntityProp} = _mapper.Map<{updateDto}>({updatedEntityProp}); // map the {entityNameLowercase} we got from the database to an updatable {entityNameLowercase} model
            request.PatchDoc.ApplyTo({patchedEntityProp}); // apply patchdoc updates to the updatable {entityNameLowercase}

            _mapper.Map({patchedEntityProp}, {updatedEntityProp});
            await _db.SaveChangesAsync();

            return true;
        }}
    }}
}}";
        }
    }
}