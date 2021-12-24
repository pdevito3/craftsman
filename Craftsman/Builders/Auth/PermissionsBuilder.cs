namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class PermissionsBuilder
    {
        public static void GetPermissions(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "Permissions.cs", projectBaseName);
            var fileText = GetPermissionsText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
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
}