namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class RolesBuilder
    {
        public static void GetRoles(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "Roles.cs", projectBaseName);
            var fileText = GetRolesText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        private static string GetRolesText(string classNamespace)
        {
            return @$"namespace {classNamespace};

using System.Reflection;

public static class Roles
{{
    public const string SuperAdmin = ""SuperAdmin"";
    public const string User = ""User"";
    
    public static List<string> List()
    {{
        return typeof(Roles)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .Select(x => (string)x.GetRawConstantValue())
            .ToList();
    }}
}}";
        }
    }
}