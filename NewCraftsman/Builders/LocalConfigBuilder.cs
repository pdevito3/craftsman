namespace NewCraftsman.Builders
{
    using System.IO.Abstractions;

    public class LocalConfigBuilder
    {
        public static void CreateLocalConfig(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "LocalConfig.cs", projectBaseName);
            var fileText = GetConfigText(classPath.ClassNamespace);
            _utilities.CreateFile(classPath, fileText);
        }
        
        private static string GetConfigText(string classNamespace)
        {
            return @$"namespace {classNamespace};

public static class LocalConfig
{{
    public const string IntegrationTestingEnvName = ""LocalIntegrationTesting"";
    public const string FunctionalTestingEnvName = ""LocalFunctionalTesting"";
}}";
        }
    }
}