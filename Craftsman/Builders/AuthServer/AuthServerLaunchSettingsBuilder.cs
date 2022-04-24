namespace Craftsman.Builders.AuthServer;

using Helpers;
using Services;

public class AuthServerLaunchSettingsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public AuthServerLaunchSettingsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateLaunchSettings(string projectDirectory, string authServerProjectName, int authServerPort)
    {
        var classPath = ClassPathHelper.AuthServerLaunchSettingsClassPath(projectDirectory, $"launchSettings.json", authServerProjectName);
        var fileText = GetLaunchSettingsText(authServerPort);
        _utilities.CreateFile(classPath, fileText);
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
