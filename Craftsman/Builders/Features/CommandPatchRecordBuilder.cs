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
        var repoInterface = FileNames.EntityRepositoryInterface(entity.Name);
        var repoInterfaceProp = $"{entity.Name.LowercaseFirstLetter()}Repository";

        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "");
        var validatorsClassPath = ClassPathHelper.ValidationClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);

        return @$"namespace {classNamespace};

using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};
using {validatorsClassPath.ClassNamespace};
using {entityServicesClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using AutoMapper;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;

public static class {className}
{{
    public class {patchCommandName} : IRequest<bool>
    {{
        public readonly {primaryKeyPropType} {primaryKeyPropName};
        public readonly JsonPatchDocument<{updateDto}> PatchDoc;

        public {patchCommandName}({primaryKeyPropType} {entityNameLowercase}, JsonPatchDocument<{updateDto}> patchDoc)
        {{
            {primaryKeyPropName} = {entityNameLowercase};
            PatchDoc = patchDoc;
        }}
    }}

    public class Handler : IRequestHandler<{patchCommandName}, bool>
    {{
        private readonly {repoInterface} _{repoInterfaceProp};
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public Handler({repoInterface} {repoInterfaceProp}, IUnitOfWork unitOfWork, IMapper mapper)
        {{
            _mapper = mapper;
            _{repoInterfaceProp} = {repoInterfaceProp};
            _unitOfWork = unitOfWork;
        }}

        public async Task<bool> Handle({patchCommandName} request, CancellationToken cancellationToken)
        {{
            if (request.PatchDoc == null)
                throw new ValidationException(
                    new List<ValidationFailure>()
                    {{
                        new ValidationFailure(""Patch Document"",""Invalid patch doc."")
                    }});

            var {updatedEntityProp} = await _{repoInterfaceProp}.GetById(request.Id, cancellationToken: cancellationToken);

            var {patchedEntityProp} = _mapper.Map<{updateDto}>({updatedEntityProp});
            request.PatchDoc.ApplyTo({patchedEntityProp});

            {updatedEntityProp}.Update({patchedEntityProp});
            await _unitOfWork.CommitChanges(cancellationToken);

            return true;
        }}
    }}
}}";
    }
}
