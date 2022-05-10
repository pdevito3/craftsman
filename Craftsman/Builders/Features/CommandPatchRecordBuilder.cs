namespace Craftsman.Builders.Features;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class CommandPatchRecordBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public CommandPatchRecordBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateCommand(string solutionDirectory, string srcDirectory, Entity entity, string contextName, string projectBaseName)
    {
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{FileNames.PatchEntityFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);
        var fileText = GetCommandFileText(classPath.ClassNamespace, entity, contextName, solutionDirectory, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetCommandFileText(string classNamespace, Entity entity, string contextName, string solutionDirectory, string srcDirectory, string projectBaseName)
    {
        var className = FileNames.PatchEntityFeatureClassName(entity.Name);
        var patchCommandName = FileNames.CommandPatchName(entity.Name);
        var updateDto = FileNames.GetDtoName(entity.Name, Dto.Update);
        var manipulationValidator = FileNames.ValidatorNameGenerator(entity.Name, Validator.Manipulation);

        var primaryKeyPropType = Entity.PrimaryKeyProperty.Type;
        var primaryKeyPropName = Entity.PrimaryKeyProperty.Name;
        var entityNameLowercase = entity.Name.LowercaseFirstLetter();
        var updatedEntityProp = $"{entityNameLowercase}ToUpdate";
        var patchedEntityProp = $"{entityNameLowercase}ToPatch";

        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "");
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
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

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
                throw new ValidationException(
                    new List<ValidationFailure>()
                    {{
                        new ValidationFailure(""Patch Document"",""Invalid patch doc."")
                    }});

            var {updatedEntityProp} = await _db.{entity.Plural}
                .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.{primaryKeyPropName} == request.{primaryKeyPropName}, cancellationToken);

            if ({updatedEntityProp} == null)
                throw new NotFoundException(""{entity.Name}"", request.{primaryKeyPropName});

            var {patchedEntityProp} = _mapper.Map<{updateDto}>({updatedEntityProp}); // map the {entityNameLowercase} we got from the database to an updatable {entityNameLowercase} dto
            request.PatchDoc.ApplyTo({patchedEntityProp}); // apply patchdoc updates to the updatable {entityNameLowercase} dto

            {updatedEntityProp}.Update({patchedEntityProp});
            await _db.SaveChangesAsync(cancellationToken);

            return true;
        }}
    }}
}}";
    }
}
