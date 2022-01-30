namespace Craftsman.Builders.Auth
{
    using System.IO.Abstractions;
    using Helpers;

    public class RolesBuilder
    {
        public static void GetRoles(string solutionDirectory, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.SharedKernelDomainClassPath(solutionDirectory, "Roles.cs");
            var fileText = GetRolesText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        private static string GetRolesText(string classNamespace)
        {
            return @$"namespace {classNamespace}
{{
    using System.Collections.Generic;
    using System.Linq;
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
    }}
}}";
        }
    }
}