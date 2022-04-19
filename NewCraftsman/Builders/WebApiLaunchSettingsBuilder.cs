namespace NewCraftsman.Builders
{
    using System.IO.Abstractions;

    public class WebApiLaunchSettingsBuilder
    {
        public static void CreateLaunchSettings(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiLaunchSettingsClassPath(srcDirectory, $"launchSettings.json", projectBaseName);
            var fileText = GetLaunchSettingsText();
            _utilities.CreateFile(classPath, fileText);
        }

        public static string GetLaunchSettingsText()
        {
            return @$"{{
  ""profiles"": {{
  }}
}}";
        }
    }
}