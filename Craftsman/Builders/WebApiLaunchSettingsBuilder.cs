namespace Craftsman.Builders
{
    using Helpers;
    using Services;

    public class WebApiLaunchSettingsBuilder
    {
        private readonly ICraftsmanUtilities _utilities;

        public WebApiLaunchSettingsBuilder(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }

        public void CreateLaunchSettings(string srcDirectory, string projectBaseName)
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