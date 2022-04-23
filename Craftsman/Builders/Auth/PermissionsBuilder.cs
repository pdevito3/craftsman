namespace Craftsman.Builders.Auth;

using Helpers;
using Services;

public class PermissionsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public PermissionsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }
    
    public void GetPermissions(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "Permissions.cs", projectBaseName);
        var fileText = GetPermissionsText(classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
    }
    
    private static string GetPermissionsText(string classNamespace)
    {
        return @$"namespace {classNamespace};

using System.Reflection;

public static class Permissions
{{
    // Permissions marker - do not delete this comment
    
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
