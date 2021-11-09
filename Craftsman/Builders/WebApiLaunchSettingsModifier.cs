namespace Craftsman.Builders
{
    using System;
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
        }

        private static string GetProfileText(ApiEnvironment env, int port)
        {
            var connectionString = String.IsNullOrEmpty(env.ConnectionString) ? "local" : env.ConnectionString.Replace(@"\", @"\\");
            
                return $@"
    ""{env.ProfileName ?? env.EnvironmentName}"": {{
      ""commandName"": ""Project"",
      ""launchBrowser"": true,
      ""launchUrl"": ""swagger"",
      ""environmentVariables"": {{
        ""ASPNETCORE_ENVIRONMENT"": ""{env.EnvironmentName}"",
        ""AUTH_AUDIENCE"": ""{env.Audience}"",
        ""AUTH_AUTHORITY"": ""{env.Authority}"",
        ""AUTH_AUTHORIZATION_URL"": ""{env.AuthorizationUrl}"",
        ""AUTH_TOKEN_URL"": ""{env.TokenUrl}"",
        ""AUTH_CLIENT_ID"": ""{env.ClientId}"",
        ""AUTH_CLIENT_SECRET"": ""{env.ClientSecret}"",
        ""DB_CONNECTION_STRING"": ""{connectionString}"",
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

