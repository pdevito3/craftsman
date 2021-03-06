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

    public class WebApiAppSettingsBuilder
    {
        /// <summary>
        /// this build will create environment based app settings files.
        /// </summary>
        public static void CreateAppSettings(string solutionDirectory, ApiEnvironment env, string dbName, string projectBaseName = "")
        {
            try
            {
                var appSettingFilename = Utilities.GetAppSettingsName(env.EnvironmentName);
                var classPath = ClassPathHelper.WebApiAppSettingsClassPath(solutionDirectory, $"{appSettingFilename}", projectBaseName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    File.Delete(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetAppSettingsText(env, dbName);
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

        /// <summary>
        /// this build will only do a skeleton app settings for the initial project build.
        /// </summary>
        /// <param name="solutionDirectory"></param>
        public static void CreateAppSettings(string solutionDirectory, string projectBaseName)
        {
            try
            {
                var appSettingFilename = "appsettings.json";
                var classPath = ClassPathHelper.WebApiAppSettingsClassPath(solutionDirectory, $"{appSettingFilename}", projectBaseName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    File.Delete(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetAppSettingsText();
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

        private static string GetAppSettingsText(ApiEnvironment env, string dbName)
        {
            var jwtSettings = GetJwtAuthSettings(env);
            var serilogSettings = GetSerilogSettings(env.EnvironmentName);

            if(env.EnvironmentName == "Development" || env.EnvironmentName == "IntegrationTesting")

                return @$"{{
  ""AllowedHosts"": ""*"",
  ""UseInMemoryDatabase"": true,
{serilogSettings}{jwtSettings}
}}
";
        else

            return @$"{{
  ""AllowedHosts"": ""*"",
  ""UseInMemoryDatabase"": false,
  ""ConnectionStrings"": {{
    ""{dbName}"": ""{env.ConnectionString.Replace(@"\",@"\\")}""
  }},
}}
";
        }

        private static string GetJwtAuthSettings(ApiEnvironment env)
        {
            return $@"
  ""JwtSettings"": {{
    ""Audience"": ""{env.Audience}"",
    ""Authority"": ""{env.Authority}"",
    ""AuthorizationUrl"": ""{env.AuthorizationUrl}"",
    ""TokenUrl"": ""{env.TokenUrl}"",
    ""ClientId"": ""{env.ClientId}"",
    ""ClientSecret"": ""{env.ClientSecret}""
  }},";
        }

        private static string GetSerilogSettings(string envName)
        {
            var writeTo = envName == "Development" || envName == "IntegrationTesting" ? $@"
      {{ ""Name"": ""Console"" }},
      {{
        ""Name"": ""Seq"",
        ""Args"": {{
          ""serverUrl"": ""http://localhost:5341""
        }}
      }}
    " : "";

            return $@"  ""Serilog"": {{
    ""Using"": [],
    ""MinimumLevel"": {{
      ""Default"": ""Information"",
      ""Override"": {{
        ""Microsoft"": ""Warning"",
        ""System"": ""Warning""
      }}
    }},
    ""Enrich"": [ ""FromLogContext"", ""WithMachineName"", ""WithProcessId"", ""WithThreadId"" ],
    ""WriteTo"": [{writeTo}]
  }},";
        }

        private static string GetAppSettingsText()
        {
            return @$"{{
  ""UseInMemoryDatabase"": true,
  ""Logging"": {{
    ""LogLevel"": {{
      ""Default"": ""Information"",
      ""Microsoft"": ""Warning"",
      ""Microsoft.Hosting.Lifetime"": ""Information""
    }}
  }},
  ""AllowedHosts"": ""*""
}}";
        }
    }
}
