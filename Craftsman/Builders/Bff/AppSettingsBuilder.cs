namespace Craftsman.Builders.Bff
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class AppSettingsBuilder
    {
        public static void CreateBffAppSettings(string projectDirectory, string projectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.AuthServerAppSettingsClassPath(projectDirectory, $"appsettings.json", projectName);
            var fileText = @$"{{
  ""AllowedHosts"": ""*""
}}
";
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
    }
}