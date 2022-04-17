namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System.IO.Abstractions;
    using System.Text;

    public class WebApiLaunchSettingsBuilder
    {
        public static void CreateLaunchSettings(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiLaunchSettingsClassPath(srcDirectory, $"launchSettings.json", projectBaseName);
            var fileText = GetLaunchSettingsText();
            Utilities.CreateFile(classPath, fileText, fileSystem);
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