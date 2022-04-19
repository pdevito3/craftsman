namespace NewCraftsman.Builders.Bff;

using System.IO.Abstractions;
using Domain;
using Helpers;
using Services;

public class LaunchSettingsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public LaunchSettingsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateLaunchSettings(string projectDirectory, string projectName, BffTemplate template)
    {
        var classPath = ClassPathHelper.BffLaunchSettingsClassPath(projectDirectory, $"launchSettings.json", projectName);
        var fileText = GetLaunchSettingsText(template);
        _utilities.CreateFile(classPath, fileText);
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
