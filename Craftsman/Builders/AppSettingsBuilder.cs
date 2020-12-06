namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class AppSettingsBuilder
    {
        public static void CreateAppSettings(string solutionDirectory, ApiEnvironment env, string dbName, ApiTemplate template)
        {
            try
            {
                var appSettingFilename = env.EnvironmentName == "Startup" ? "" : Utilities.GetAppSettingsName(env.EnvironmentName);
                var classPath = ClassPathHelper.AppSettingsClassPath(solutionDirectory, $"{appSettingFilename}");

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetAppSettingsText(env, dbName, template);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
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

        private static string GetAppSettingsText(ApiEnvironment env, string dbName, ApiTemplate template)
        {
            var jwtSettings = "";
            var mailSettings = "";
            
            if(template.AuthSetup.AuthMethod == "JWT")
            {
                jwtSettings = $@"
  ""JwtSettings"": {{
    ""Key"": ""{env.JwtSettings.Key}"",
    ""Issuer"": ""{env.JwtSettings.Issuer}"",
    ""Audience"": ""{env.JwtSettings.Audience}"",
    ""DurationInMinutes"": {env.JwtSettings.DurationInMinutes}
  }},";
            }

            mailSettings = @$"
  ""MailSettings"": {{
    ""EmailFrom"": ""{env.MailSettings.EmailFrom}"",
    ""SmtpHost"": ""{env.MailSettings.SmtpHost}"",
    ""SmtpPort"": ""{env.MailSettings.SmtpPort}"",
    ""SmtpUser"": ""{env.MailSettings.SmtpUser}"",
    ""SmtpPass"": ""{env.MailSettings.SmtpPass}"",
    ""DisplayName"": ""{env.MailSettings.DisplayName}""
  }},";

            if(env.EnvironmentName == "Development")

                return @$"{{
  ""UseInMemoryDatabase"": true,
  ""Logging"": {{
    ""LogLevel"": {{
      ""Default"": ""Information"",
      ""Microsoft"": ""Warning"",
      ""Microsoft.Hosting.Lifetime"": ""Information""
    }}
  }},{jwtSettings}{mailSettings}
  ""AllowedHosts"": ""*""
}}
";
        else

            return @$"{{
  ""UseInMemoryDatabase"": false,
  ""ConnectionStrings"": {{
    ""{dbName}"": ""{env.ConnectionString.Replace(@"\",@"\\")}""
  }},
  ""Logging"": {{
    ""LogLevel"": {{
      ""Default"": ""Information"",
      ""Microsoft"": ""Warning"",
      ""Microsoft.Hosting.Lifetime"": ""Information""
    }}
  }},
  ""AllowedHosts"": ""*""
}}
";
        }
    }
}
