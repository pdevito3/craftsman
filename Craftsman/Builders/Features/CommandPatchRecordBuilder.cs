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

    public void CreateCommand(string srcDirectory, Entity entity, string projectBaseName, bool isProtected, string permissionName)
    {
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{FileNames.PatchEntityFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);
        var fileText = GetCommandFileText(classPath.ClassNamespace, entity, srcDirectory, projectBaseName, isProtected,
            permissionName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetCommandFileText(string classNamespace, Entity entity, string srcDirectory, string projectBaseName, bool isProtected, string permissionName)
    {
        var className = FileNames.PatchEntityFeatureClassName(entity.Name);
        var patchCommandName = FileNames.CommandPatchName();
        var updateDto = FileNames.GetDtoName(entity.Name, Dto.Update);

        var primaryKeyPropType = Entity.PrimaryKeyProperty.Type;
        var primaryKeyPropName = Entity.PrimaryKeyProperty.Name;
        var entityNameLowercase = entity.Name.LowercaseFirstLetter();
        var updatedEntityProp = $"{entityNameLowercase}ToUpdate";
        var patchedEntityProp = $"{entityNameLowercase}ToPatch";
        var repoInterface = FileNames.EntityRepositoryInterface(entity.Name);
        var repoInterfaceProp = $"{entity.Name.LowercaseFirstLetter()}Repository";
        
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "");
        
        FeatureBuilderHelpers.GetPermissionValuesForHandlers(srcDirectory, 
            projectBaseName, 
            isProtected, 
            permissionName, 
            out string heimGuardSetter, 
            out string heimGuardCtor, 
            out string permissionCheck, 
            out string permissionsUsing,
            out string heimGuardField);

        return @$"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using {entityServicesClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};{permissionsUsing}
using MapsterMapper;
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
        private readonly IMapper _mapper;{heimGuardField}

        public Handler({repoInterface} {repoInterfaceProp}, IUnitOfWork unitOfWork, IMapper mapper{heimGuardCtor})
        {{
            _mapper = mapper;
            _{repoInterfaceProp} = {repoInterfaceProp};
            _unitOfWork = unitOfWork;{heimGuardSetter}
        }}

        public async Task<bool> Handle({patchCommandName} request, CancellationToken cancellationToken)
        {{{permissionCheck}
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
