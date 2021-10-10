namespace Craftsman.Builders.AuthServer
{
    using System.IO.Abstractions;
    using Helpers;

    public class AuthServerLaunchSettingsBuilder
    {
        public static void CreateLaunchSettings(string projectDirectory, string authServerProjectName, int authServerPort, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.AuthServerLaunchSettingsClassPath(projectDirectory, $"launchSettings.json", authServerProjectName);
            var fileText = GetLaunchSettingsText(authServerPort);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static string GetLaunchSettingsText(int authServerPort)
        {
            return @$"{{
  ""profiles"": {{
    ""SelfHost"": {{
      ""commandName"": ""Project"",
      ""launchBrowser"": true,
      ""environmentVariables"": {{
        ""ASPNETCORE_ENVIRONMENT"": ""Development""
      }},
      ""applicationUrl"": ""https://localhost:{authServerPort}""
    }}
  }}
}}";
        }
    }
}