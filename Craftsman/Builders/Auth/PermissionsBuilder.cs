namespace Craftsman.Builders.Auth;

using Helpers;
using Services;
using MediatR;

public static class PermissionsBuilder
{
    public sealed record Command(bool HasAuth) : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.PolicyDomainClassPath(scaffoldingDirectoryStore.SrcDirectory, "Permissions.cs", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetPermissionsText(classPath.ClassNamespace, request.HasAuth);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string GetPermissionsText(string classNamespace, bool hasAuth)
    {
        var rolePermissions = "";
        if (hasAuth)
            rolePermissions += $@"
    public const string CanRemoveUserRoles = nameof(CanRemoveUserRoles);
    public const string CanAddUserRoles = nameof(CanAddUserRoles);
    public const string CanGetRoles = nameof(CanGetRoles);
    public const string CanGetPermissions = nameof(CanGetPermissions);";
        
        return @$"namespace {classNamespace};

using System.Reflection;

public static class Permissions
{{
    // Permissions marker - do not delete this comment
    public const string HangfireAccess = nameof(HangfireAccess);{rolePermissions}
    
    public static List<string> List()
    {{
        return typeof(Permissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .Select(x => (string)x.GetRawConstantValue())
            .ToList();
    }}
}}";
    }
}
