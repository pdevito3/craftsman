namespace NewCraftsman.Builders.Bff
{
    using System.IO.Abstractions;
    using Helpers;
    using Services;

    public class AppSettingsBuilder
    {
        private readonly ICraftsmanUtilities _utilities;

        public AppSettingsBuilder(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }

        public void CreateBffAppSettings(string projectDirectory, string projectName)
        {
            var classPath = ClassPathHelper.BffProjectRootClassPath(projectDirectory, $"appsettings.json", projectName);
            var fileText = @$"{{
  ""AllowedHosts"": ""*""
}}
";
            _utilities.CreateFile(classPath, fileText);
        }
    }
}