namespace Craftsman.Builders;

using Helpers;
using Services;

public class ConstsResourceBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public ConstsResourceBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateLocalConfig(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "Consts.cs", projectBaseName);
        var fileText = GetConfigText(classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetConfigText(string classNamespace)
    {
        return @$"namespace {classNamespace};

using System.Reflection;

public static class Consts
{{
    public static class Testing
    {{
        public const string IntegrationTestingEnvName = ""LocalIntegrationTesting"";
        public const string FunctionalTestingEnvName = ""LocalFunctionalTesting"";
    }}

    public static class HangfireQueues
    {{
        public const string Default = ""default"";
        // public const string MyFirstQueue = ""my-first-queue"";
        
        public static string[] List()
        {{
            return typeof(HangfireQueues)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                .Select(x => (string)x.GetRawConstantValue())
                .ToArray();
        }}
    }}
}}";
    }
}
