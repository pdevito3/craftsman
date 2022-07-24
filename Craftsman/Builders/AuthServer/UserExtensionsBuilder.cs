namespace Craftsman.Builders.AuthServer;

using Craftsman.Helpers;
using Craftsman.Services;

public class UserExtensionsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public UserExtensionsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void Create(string solutionDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.AuthServerExtensionsClassPath(solutionDirectory, "UserExtensions.cs", projectBaseName);
        var fileText = GetFileText(classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
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
