namespace Craftsman.Builders;

using Domain;
using Helpers;
using Services;

public class AppSettingsDevelopmentBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public AppSettingsDevelopmentBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    /// <summary>
    /// this build will create environment based app settings files.
    /// </summary>
    public void CreateWebApiAppSettings(string srcDirectory, ApiEnvironment env, DockerConfig dockerConfig, string projectBaseName)
    {
        var appSettingFilename = FileNames.GetAppSettingsName(true);
        var classPath = ClassPathHelper.WebApiAppSettingsClassPath(srcDirectory, $"{appSettingFilename}", projectBaseName);
        var fileText = GetAppSettingsText(env, dockerConfig, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetAppSettingsText(ApiEnvironment env, DockerConfig dockerConfig, string projectBaseName)
    {
        return @$"{{
  ""AllowedHosts"": ""*"",
  ""ConnectionStrings"": {{
    ""{CraftsmanUtilities.GetCleanProjectName(projectBaseName)}"": ""{dockerConfig.DbConnectionString}""
  }},
  ""Auth"": {{
    ""Audience"": ""{env.AuthSettings.Audience}"",
    ""Authority"": ""{env.AuthSettings.Authority}"",
    ""AuthorizationUrl"": ""{env.AuthSettings.AuthorizationUrl}"",
    ""TokenUrl"": ""{env.AuthSettings.TokenUrl}"",
    ""ClientId"": ""{env.AuthSettings.ClientId}"",
    ""ClientSecret"": ""{env.AuthSettings.ClientSecret}""
  }},
  ""RabbitMq"": {{
    ""Host"": ""{env.BrokerSettings.Host}"",
    ""VirtualHost"": ""{env.BrokerSettings.VirtualHost}"",
    ""Username"": ""{env.BrokerSettings.Username}"",
    ""Password"": ""{env.BrokerSettings.Password}"",
    ""Port"": ""{env.BrokerSettings.BrokerPort}"",
  }},
  ""JaegerHost"": ""localhost""
}}
";
    }
}
