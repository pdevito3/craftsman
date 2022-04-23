namespace Craftsman.Builders.Auth
{
    using Helpers;
    using Services;

    public class RolesBuilder
    {
        private readonly ICraftsmanUtilities _utilities;

        public RolesBuilder(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }
        
        public void GetRoles(string solutionDirectory)
        {
            var classPath = ClassPathHelper.SharedKernelDomainClassPath(solutionDirectory, "Roles.cs");
            var fileText = GetRolesText(classPath.ClassNamespace);
            _utilities.CreateFile(classPath, fileText);
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