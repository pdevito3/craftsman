namespace Craftsman.Builders
{
    using System.IO.Abstractions;
    using Helpers;

    public class LocalConfigBuilder
    {
        public static void CreateLocalConfig(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiUtilsClassPath(srcDirectory, "LocalConfig.cs", projectBaseName);
            var fileText = GetConfigText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        private static string GetConfigText(string classNamespace)
        {
            return @$"namespace {classNamespace}
{{
    public static class LocalConfig
    {{
        public const string IntegrationTestingEnvName = ""LocalIntegrationTesting"";
        public const string FunctionalTestingEnvName = ""LocalFunctionalTesting"";
    }}
}}";
        }
    }
}