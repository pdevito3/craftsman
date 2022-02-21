namespace Craftsman.Builders
{
    using System;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using Enums;

    public class WebApiLaunchSettingsModifier
    {
        public static void AddProfile(string solutionDirectory, ApiEnvironment env, int port, DockerConfig dockerConfig, string projectBaseName)
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
                            newText += GetProfileText(env, port, dockerConfig);
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);
        }

        private static string GetProfileText(ApiEnvironment env, int port, DockerConfig dockerConfig)
        {
            return $@"
    ""{env.ProfileName ?? env.EnvironmentName}"": {{
      ""commandName"": ""Project"",
      ""launchBrowser"": true,
      ""launchUrl"": ""swagger"",
      ""environmentVariables"": {{
        ""ASPNETCORE_ENVIRONMENT"": ""{env.EnvironmentName}"",
        ""AUTH_AUDIENCE"": ""{env.AuthSettings.Audience}"",
        ""AUTH_AUTHORITY"": ""{env.AuthSettings.Authority}"",
        ""AUTH_AUTHORIZATION_URL"": ""{env.AuthSettings.AuthorizationUrl}"",
        ""AUTH_TOKEN_URL"": ""{env.AuthSettings.TokenUrl}"",
        ""AUTH_CLIENT_ID"": ""{env.AuthSettings.ClientId}"",
        ""AUTH_CLIENT_SECRET"": ""{env.AuthSettings.ClientSecret}"",
        ""DB_CONNECTION_STRING"": ""{dockerConfig.DbConnectionString}"",
        ""RMQ_HOST"": ""{env.BrokerSettings.Host}"",
        ""RMQ_VIRTUAL_HOST"": ""{env.BrokerSettings.VirtualHost}"",
        ""RMQ_USERNAME"": ""{env.BrokerSettings.Username}"",
        ""RMQ_PASSWORD"": ""{env.BrokerSettings.Password}""        
      }},
      ""applicationUrl"": ""https://localhost:{port}""
    }},";
        }
    }
}

