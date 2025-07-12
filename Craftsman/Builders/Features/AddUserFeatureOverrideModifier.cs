namespace Craftsman.Builders.Features;

using System.IO;
using System.IO.Abstractions;
using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;
using MediatR;

public static class AddUserFeatureOverrideModifier
{
    public sealed record UpdateAddUserFeatureCommand(string ProjectBaseName, string DbContextName) : IRequest;

    public class UpdateAddUserFeatureHandler(IFileSystem fileSystem, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<UpdateAddUserFeatureCommand>
    {
        public Task Handle(UpdateAddUserFeatureCommand request, CancellationToken cancellationToken)
        {
            UpdateAddUserFeature(scaffoldingDirectoryStore.SrcDirectory, request.ProjectBaseName, request.DbContextName, fileSystem);
            return Task.CompletedTask;
        }
    }

    private static void UpdateAddUserFeature(string srcDirectory, string projectBaseName, string dbContextName, IFileSystem fileSystem)
    {
        var entityName = "User";
        var entityPlural = "Users";
        var permissionName = FeatureType.AddRecord.DefaultPermission("Users", "AddUser");
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{FileNames.AddEntityFeatureClassName(entityName)}.cs", entityPlural, projectBaseName);

        if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
            fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

        var className = FileNames.AddEntityFeatureClassName(entityName);
        var addCommandName = FileNames.CommandAddName();
        var readDto = FileNames.GetDtoName(entityName, Dto.Read);
        var createDto = FileNames.GetDtoName(entityName, Dto.Creation);

        var entityNameLowercase = entityName.LowercaseFirstLetter();
        var commandProp = $"{entityName}ToAdd";
        var newEntityProp = $"{entityNameLowercase}ToAdd";
        var repoInterface = FileNames.EntityRepositoryInterface(entityName);
        var repoInterfaceProp = $"{entityName.LowercaseFirstLetter()}Repository";
        var modelToCreateVariableName = $"{entityName.LowercaseFirstLetter()}ToAdd";

        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);
        var modelClassPath = ClassPathHelper.EntityModelClassPath(srcDirectory, entityName, entityPlural, null, projectBaseName);
        var dbContextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
        
        FeatureBuilderHelpers.GetPermissionValuesForHandlers(srcDirectory, 
            projectBaseName, 
            true, 
            permissionName, 
            out string heimGuardCtor, 
            out string permissionCheck, 
            out string permissionsUsing);

        var tempPath = $"{classPath.FullClassPath}temp";
        using var output = fileSystem.File.CreateText(tempPath);
        {
            output.WriteLine($@"namespace {classPath.ClassNamespace};

using {dbContextClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {modelClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};{permissionsUsing}
using Mappings;
using MediatR;

public static class {className}
{{
    public sealed record {addCommandName}({createDto} {commandProp}, bool SkipPermissions = false) : IRequest<{readDto}>;

    public sealed class Handler({dbContextName} dbContext{heimGuardCtor})
        : IRequestHandler<{addCommandName}, {readDto}>
    {{
        public async Task<{readDto}> Handle(Command request, CancellationToken cancellationToken)
        {{
            if(!request.SkipPermissions)
                await heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.{permissionName});

            var {modelToCreateVariableName} = request.{commandProp}.To{EntityModel.Creation.GetClassName(entityName)}();
            var {entityNameLowercase} = {entityName}.Create({modelToCreateVariableName});
            await dbContext.{entityPlural}.AddAsync({entityNameLowercase}, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            var {entityNameLowercase}Added = await dbContext.Users.GetById({entityNameLowercase}.Id, cancellationToken);
            return {entityNameLowercase}Added.To{readDto}();
        }}
    }}
}}");
        }

        // delete the old file and set the name of the new one to the original name
        fileSystem.File.Delete(classPath.FullClassPath);
        output.Close();
        fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }
}
