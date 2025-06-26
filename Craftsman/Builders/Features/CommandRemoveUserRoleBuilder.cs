namespace Craftsman.Builders.Features;

using Domain;
using Domain.Enums;
using Helpers;
using MediatR;
using Services;

public static class CommandRemoveUserRoleBuilder
{
    public class CommandRemoveUserRoleBuilderCommand(Entity entity, string dbContextName) : IRequest<bool>
    {
        public Entity Entity { get; set; } = entity;
        public string DbContextName { get; set; } = dbContextName;
    }

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<CommandRemoveUserRoleBuilderCommand, bool>
    {
        public Task<bool> Handle(CommandRemoveUserRoleBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.FeaturesClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{FileNames.RemoveUserRoleFeatureClassName()}.cs", request.Entity.Plural, scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetCommandFileText(classPath.ClassNamespace, request.Entity, scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.ProjectBaseName, request.DbContextName);
            utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private static string GetCommandFileText(string classNamespace, Entity entity, string srcDirectory, string projectBaseName, string dbContextName)
    {
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);
        var dbContextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);

            // lang=csharp
            return $$"""
                     namespace {{classNamespace}};

                     using {{dbContextClassPath.ClassNamespace}};
                     using {{exceptionsClassPath.ClassNamespace}};
                     using HeimGuard;
                     using MediatR;
                     using Roles;

                     public static class {{FileNames.RemoveUserRoleFeatureClassName()}}
                     {
                         public sealed record Command(Guid UserId, string Role) : IRequest;

                         public sealed class Handler({{dbContextName}} dbContext,
                             IHeimGuardClient heimGuard) : IRequestHandler<Command>
                         {
                             public async Task Handle(Command request, CancellationToken cancellationToken)
                             {
                                 await heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanRemoveUserRoles);
                                 var user = await dbContext.GetUserAggregate().GetById(request.UserId, cancellationToken);

                                 var roleToRemove = user.RemoveRole(new Role(request.Role));
                                 dbContext.UserRoles.Remove(roleToRemove);
                                 dbContext.Update(user);

                                 await dbContext.SaveChangesAsync(cancellationToken);
                             }
                         }
                     }
                     """;
        }
    }
}
