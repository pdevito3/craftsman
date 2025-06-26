namespace Craftsman.Builders.Features;

using Domain;
using Domain.Enums;
using Helpers;
using MediatR;
using Services;

public static class CommandAddUserRoleBuilder
{
    public class CommandAddUserRoleBuilderCommand(Entity entity, string dbContextName) : IRequest<bool>
    {
        public Entity Entity { get; set; } = entity;
        public string DbContextName { get; set; } = dbContextName;
    }

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<CommandAddUserRoleBuilderCommand, bool>
    {
        public Task<bool> Handle(CommandAddUserRoleBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.FeaturesClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{FileNames.AddUserRoleFeatureClassName()}.cs", request.Entity.Plural, scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetCommandFileText(classPath.ClassNamespace, request.Entity, scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.ProjectBaseName, request.DbContextName);
            utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private static string GetCommandFileText(string classNamespace, Entity entity, string srcDirectory, string projectBaseName, string dbContextName)
        {
            var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entity.Plural, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
            var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
            var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);
            var dbContextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);

            // lang=csharp
            return $$"""
                     namespace {{classNamespace}};

                     using {{dbContextClassPath.ClassNamespace}};
                     using {{entityClassPath.ClassNamespace}};
                     using {{dtoClassPath.ClassNamespace}};
                     using {{servicesClassPath.ClassNamespace}};
                     using {{exceptionsClassPath.ClassNamespace}};
                     using HeimGuard;
                     using Mappings;
                     using MediatR;
                     using Roles;

                     public static class {{FileNames.AddUserRoleFeatureClassName()}}
                     {
                         public sealed record Command(Guid UserId, string Role, bool SkipPermissions = false) : IRequest;

                         public sealed class Handler({{dbContextName}} dbContext, IHeimGuardClient heimGuard) : IRequestHandler<Command>
                         {
                             public async Task Handle(Command request, CancellationToken cancellationToken)
                             {
                                 if(!request.SkipPermissions)
                                     await heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddUserRoles);
                                 
                                 var user = await dbContext.GetUserAggregate().GetById(request.UserId, cancellationToken);

                                 var roleToAdd = user.AddRole(new Role(request.Role));
                                 await dbContext.UserRoles.AddAsync(roleToAdd, cancellationToken);

                                 await dbContext.SaveChangesAsync(cancellationToken);
                             }
                         }
                     }
                     """;
        }
    }
}
