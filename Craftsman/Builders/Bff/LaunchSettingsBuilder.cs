namespace Craftsman.Builders.Bff;

using System.IO.Abstractions;
using Helpers;
using Models;

public class LaunchSettingsBuilder
{
    public static void CreateLaunchSettings(string projectDirectory, string projectName, BffTemplate template, IFileSystem fileSystem)
    {
        var classPath = ClassPathHelper.BffLaunchSettingsClassPath(projectDirectory, $"launchSettings.json", projectName);
        var fileText = GetLaunchSettingsText(template);
        Utilities.CreateFile(classPath, fileText, fileSystem);
    }

    public static string GetLaunchSettingsText(BffTemplate template)
    {
            return @$"{{
  ""profiles"": {{
    ""{template.ProjectName}"": {{
      ""commandName"": ""Project"",
      ""launchBrowser"": true,
      ""applicationUrl"": ""https://localhost:{template.Port};"",
      ""environmentVariables"": {{
        ""ASPNETCORE_ENVIRONMENT"": ""Development"",
        ""ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"": ""Microsoft.AspNetCore.SpaProxy"",
        ""AUTH_AUTHORITY"": ""{template.Authority}"",
        ""AUTH_CLIENT_ID"": ""{template.ClientId}"",
        ""AUTH_CLIENT_SECRET"": ""{template.ClientSecret}""
      }}
    }}
  }}
}}";
    }
}
