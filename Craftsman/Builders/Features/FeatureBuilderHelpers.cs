namespace Craftsman.Builders.Features;

using Services;

public class FeatureBuilderHelpers
{
    public static void GetPermissionValuesForHandlers(string srcDirectory, string projectBaseName, bool isProtected,
        string permissionName, out string heimGuardSetter, out string heimGuardCtor, out string permissionCheck,
        out string permissionsUsing, out string heimGuardField)
    {
        var permissionsClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);
        heimGuardField = isProtected ? $"{Environment.NewLine}        private readonly IHeimGuardClient _heimGuard;" : null;
        heimGuardSetter = isProtected ? $"{Environment.NewLine}            _heimGuard = heimGuard;" : null;
        heimGuardCtor = isProtected ? $", IHeimGuardClient heimGuard" : null;
        permissionCheck = isProtected
            ? $"{Environment.NewLine}            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.{permissionName});{Environment.NewLine}"
            : null;
        permissionsUsing = isProtected
            ? $@"
using {permissionsClassPath.ClassNamespace};
using HeimGuard;"
            : null;
    }
}