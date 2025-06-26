namespace Craftsman.Builders.Features;

using Domain;
using Domain.Enums;
using Helpers;
using Services;
using MediatR;

public static class CommandPatchRecordBuilder
{
    public class Command(Entity entity, bool isProtected, string permissionName, string dbContextName) : IRequest<bool>
    {
        public Entity Entity { get; set; } = entity;
        public bool IsProtected { get; set; } = isProtected;
        public string PermissionName { get; set; } = permissionName;
        public string DbContextName { get; set; } = dbContextName;
    }

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command, bool>
    {
        public Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.FeaturesClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{FileNames.PatchEntityFeatureClassName(request.Entity.Name)}.cs", request.Entity.Plural, scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace, request.Entity, scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.ProjectBaseName, request.IsProtected, request.PermissionName, request.DbContextName);
            utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private static string GetFileText(string classNamespace, Entity entity, string srcDirectory, string projectBaseName, bool isProtected, string permissionName, string dbContextName)
    {
        var className = FileNames.PatchEntityFeatureClassName(entity.Name);
        var patchCommandName = FileNames.CommandPatchName();
        var updateDto = FileNames.GetDtoName(entity.Name, Dto.Update);

        var primaryKeyPropType = Entity.PrimaryKeyProperty.Type;
        var primaryKeyPropName = Entity.PrimaryKeyProperty.Name;
        var primaryKeyPropNameLowercase = primaryKeyPropName.LowercaseFirstLetter();
        var entityNameLowercase = entity.Name.LowercaseFirstLetter();
        var updatedEntityProp = $"{entityNameLowercase}ToUpdate";
        var patchedEntityProp = $"{entityNameLowercase}ToPatch";
        var repoInterface = FileNames.EntityRepositoryInterface(entity.Name);
        var repoInterfaceProp = $"{entity.Name.LowercaseFirstLetter()}Repository";
        
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);
        var dbContextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
        
        FeatureBuilderHelpers.GetPermissionValuesForHandlers(srcDirectory, 
            projectBaseName, 
            isProtected, 
            permissionName, 
            out string heimGuardCtor, 
            out string permissionCheck, 
            out string permissionsUsing);

            // lang=csharp
            return $@"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using {dbContextClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};{permissionsUsing}
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;

public static class {className}
{{
    public sealed record {patchCommandName}({primaryKeyPropType} {primaryKeyPropName}, JsonPatchDocument<{updateDto}> PatchDoc) : IRequest;

    public sealed class Handler({dbContextName} dbContext{heimGuardCtor})
        : IRequestHandler<{patchCommandName}>
    {{

        public async Task Handle({patchCommandName} request, CancellationToken cancellationToken)
        {{{permissionCheck}
            if (request.PatchDoc == null)
                throw new ValidationException(
                    new List<ValidationFailure>()
                    {{
                        new ValidationFailure(""Patch Document"",""Invalid patch doc."")
                    }});

            var {updatedEntityProp} = await {repoInterfaceProp}.GetById(request.Id, cancellationToken: cancellationToken);

            var {patchedEntityProp} = {updatedEntityProp}.To{updateDto}();
            request.PatchDoc.ApplyTo({patchedEntityProp});

            {updatedEntityProp}.Update({patchedEntityProp});
            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }}
    }}
}}";
        }
    }
}
