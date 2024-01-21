namespace Craftsman.Builders.Features;

using Services;

public class FeatureBuilderHelpers
{
    public static void GetPermissionValuesForHandlers(string srcDirectory, string projectBaseName, bool isProtected,
        string permissionName, out string heimGuardCtor, out string permissionCheck,
        out string permissionsUsing)
    {
        var permissionsClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);
        heimGuardCtor = isProtected ? $", IHeimGuardClient heimGuard" : null;
        permissionCheck = isProtected
            ? $"{Environment.NewLine}            await heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.{permissionName});{Environment.NewLine}"
            : null;
        permissionsUsing = isProtected
            ? $@"
using {permissionsClassPath.ClassNamespace};
using HeimGuard;"
            : null;
    }
}