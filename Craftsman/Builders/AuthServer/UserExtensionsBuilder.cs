namespace Craftsman.Builders.AuthServer;

using Craftsman.Helpers;
using Craftsman.Services;
using MediatR;

public static class UserExtensionsBuilder
{
    public sealed record Command(string ProjectBaseName) : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.AuthServerExtensionsClassPath(scaffoldingDirectoryStore.SolutionDirectory, "UserExtensions.cs", request.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string GetFileText(string classNamespace)
    {
        return @$"namespace {classNamespace};

using Pulumi;
using Pulumi.Keycloak;

public static class UserExtensions
{{
    public static UserRoles SetRoles(this User user, params Input<string>[] userRoles)
    {{
        return new UserRoles($""user-roles-{{user.Id}}"", new UserRolesArgs()
        {{
            UserId = user.Id,
            RealmId = user.RealmId,
            RoleIds = userRoles
        }});
    }}
}}";
    }
}
