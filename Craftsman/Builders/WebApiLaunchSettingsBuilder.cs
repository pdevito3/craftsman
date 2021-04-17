namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class WebApiLaunchSettingsBuilder
    {
        public static void CreateLaunchSettings(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiLaunchSettingsClassPath(solutionDirectory, $"launchSettings.json", projectBaseName);

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (var fs = fileSystem.File.Create(classPath.FullClassPath))
            {
                var data = "";
                data = GetLaunchSettingsText();
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static string GetLaunchSettingsText()
        {
            return @$"{{
  ""iisSettings"": {{
    ""windowsAuthentication"": false,
    ""anonymousAuthentication"": true,
    ""iisExpress"": {{
                ""applicationUrl"": ""http://localhost:52117"",
      ""sslPort"": 0
    }}
        }},
  ""$schema"": ""http://json.schemastore.org/launchsettings.json"",
  ""profiles"": {{
  }}
}}";
        }
    }
}