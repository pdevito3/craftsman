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
        public static void CreateBffAppSettings(string projectDirectory, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.BffProjectRootClassPath(projectDirectory, $"appsettings.json");
            var fileText = @$"{{
  ""AllowedHosts"": ""*""
}}
";
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
    }
}