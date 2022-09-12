namespace Craftsman.Builders.Features;

using Domain;
using Helpers;
using Services;

public class CommandDeleteRecordBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public CommandDeleteRecordBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateCommand(string srcDirectory, Entity entity, string projectBaseName, bool isProtected, string permissionName)
    {
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{FileNames.DeleteEntityFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);
        var fileText = GetCommandFileText(classPath.ClassNamespace, entity, srcDirectory, projectBaseName, isProtected, permissionName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetCommandFileText(string classNamespace, Entity entity, string srcDirectory, string projectBaseName, bool isProtected, string permissionName)
    {
        var className = FileNames.DeleteEntityFeatureClassName(entity.Name);
        var deleteCommandName = FileNames.CommandDeleteName();

        var primaryKeyPropType = Entity.PrimaryKeyProperty.Type;
        var primaryKeyPropName = Entity.PrimaryKeyProperty.Name;
        var entityNameLowercase = entity.Name.LowercaseFirstLetter();
        var repoInterface = FileNames.EntityRepositoryInterface(entity.Name);
        var repoInterfaceProp = $"{entity.Name.LowercaseFirstLetter()}Repository";

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

using {entityServicesClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};{permissionsUsing}
using MediatR;

public static class {className}
{{
    public sealed class {deleteCommandName} : IRequest<bool>
    {{
        public readonly {primaryKeyPropType} {primaryKeyPropName};

        public {deleteCommandName}({primaryKeyPropType} {entityNameLowercase})
        {{
            {primaryKeyPropName} = {entityNameLowercase};
        }}
    }}

    public sealed class Handler : IRequestHandler<{deleteCommandName}, bool>
    {{
        private readonly {repoInterface} _{repoInterfaceProp};
        private readonly IUnitOfWork _unitOfWork;{heimGuardField}

        public Handler({repoInterface} {repoInterfaceProp}, IUnitOfWork unitOfWork{heimGuardCtor})
        {{
            _{repoInterfaceProp} = {repoInterfaceProp};
            _unitOfWork = unitOfWork;{heimGuardSetter}
        }}

        public async Task<bool> Handle({deleteCommandName} request, CancellationToken cancellationToken)
        {{{permissionCheck}
            var recordToDelete = await _{repoInterfaceProp}.GetById(request.Id, cancellationToken: cancellationToken);

            _{repoInterfaceProp}.Remove(recordToDelete);
            return await _unitOfWork.CommitChanges(cancellationToken) >= 1;
        }}
    }}
}}";
    }
}
