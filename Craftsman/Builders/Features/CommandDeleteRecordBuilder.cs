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

    public void CreateCommand(string solutionDirectory, string srcDirectory, Entity entity, string contextName, string projectBaseName)
    {
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{FileNames.DeleteEntityFeatureClassName(entity.Name)}.cs", entity.Plural, projectBaseName);
        var fileText = GetCommandFileText(classPath.ClassNamespace, entity, contextName, solutionDirectory, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetCommandFileText(string classNamespace, Entity entity, string contextName, string solutionDirectory, string srcDirectory, string projectBaseName)
    {
        var className = FileNames.DeleteEntityFeatureClassName(entity.Name);
        var deleteCommandName = FileNames.CommandDeleteName(entity.Name);

        var primaryKeyPropType = Entity.PrimaryKeyProperty.Type;
        var primaryKeyPropName = Entity.PrimaryKeyProperty.Name;
        var entityNameLowercase = entity.Name.LowercaseFirstLetter();
        var repoInterface = FileNames.EntityRepositoryInterface(entity.Name);
        var repoInterfaceProp = $"{entity.Name.LowercaseFirstLetter()}Repository";

        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);

        return @$"namespace {classNamespace};

using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {entityServicesClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using MediatR;

public static class {className}
{{
    public class {deleteCommandName} : IRequest<bool>
    {{
        public readonly {primaryKeyPropType} {primaryKeyPropName};

        public {deleteCommandName}({primaryKeyPropType} {entityNameLowercase})
        {{
            {primaryKeyPropName} = {entityNameLowercase};
        }}
    }}

    public class Handler : IRequestHandler<{deleteCommandName}, bool>
    {{
        private readonly {repoInterface} _{repoInterfaceProp};
        private readonly IUnitOfWork _unitOfWork;

        public Handler({repoInterface} {repoInterfaceProp}, IUnitOfWork unitOfWork)
        {{
            _{repoInterfaceProp} = {repoInterfaceProp};
            _unitOfWork = unitOfWork;
        }}

        public async Task<bool> Handle({deleteCommandName} request, CancellationToken cancellationToken)
        {{
            var recordToDelete = await _{repoInterfaceProp}.GetById(request.Id, cancellationToken: cancellationToken);

            _{repoInterfaceProp}.Remove(recordToDelete);
            await _unitOfWork.CommitChanges(cancellationToken);
            return true;
        }}
    }}
}}";
    }
}
