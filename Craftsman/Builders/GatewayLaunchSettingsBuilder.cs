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

    public class GatewayLaunchSettingsBuilder
    {
        public static void CreateLaunchSettings(string solutionDirectory, string gatewayProjectName, IFileSystem fileSystem)
        {
            try
            {
                var classPath = ClassPathHelper.GatewayLaunchSettingsClassPath(solutionDirectory, $"launchSettings.json", gatewayProjectName);

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

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{fileSystem.Path.DirectorySeparatorChar}", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
                throw;
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
