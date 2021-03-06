namespace Craftsman.Builders
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;

    public class WebApiLaunchSettingsModifier
    {
        public static void AddProfile(string solutionDirectory, ApiEnvironment env, int port, string projectBaseName = "")
        {
            var classPath = ClassPathHelper.WebApiLaunchSettingsClassPath(solutionDirectory, $"launchsettings.json", projectBaseName); // hard coding webapi here not great

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using (var output = new StreamWriter(tempPath))
                {
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains(@$"""profiles"""))
                        {
                            newText += GetProfileText(env, port);
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);

            GlobalSingleton.AddUpdatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
        }

        private static string GetProfileText(ApiEnvironment env, int port)
        {
            if (env.EnvironmentName == "Development")
                return $@"
    ""{env.ProfileName ?? "Development"}"": {{
      ""commandName"": ""Project"",
      ""launchBrowser"": true,
      ""launchUrl"": ""swagger"",
      ""environmentVariables"": {{
        ""ASPNETCORE_ENVIRONMENT"": ""{env.EnvironmentName}""
      }},
      ""applicationUrl"": ""https://localhost:{port}""
    }},";
            else if (env.EnvironmentName == "Startup")
                return $@"
    ""{env.ProfileName}"": {{
      ""commandName"": ""Project"",
      ""launchBrowser"": false
    }},";
            else
                return $@"
    ""{env.ProfileName}"": {{
      ""commandName"": ""Project"",
      ""launchBrowser"": false,
      ""environmentVariables"": {{
        ""ASPNETCORE_ENVIRONMENT"": ""{env.EnvironmentName}""
      }}
    }},";
        }
    }
}

