namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Linq;
    using static Helpers.ConsoleWriter;

    public class GatewayAppSettingsBuilder
    {
        /// <summary>
        /// this build will create environment based app settings files.
        /// </summary>
        public static void CreateAppSettings(string solutionDirectory, EnvironmentGateway env, string gatewayProjectName, List<Microservice> microservices)
        {
            try
            {
                var appSettingFilename = Utilities.GetAppSettingsName(env.EnvironmentName);
                var classPath = ClassPathHelper.GatewayAppSettingsClassPath(solutionDirectory, $"{appSettingFilename}", gatewayProjectName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    File.Delete(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetAppSettingsText(env, microservices);
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
        public static void CreateBaseAppSettings(string solutionDirectory, string gatewayProjectName)
        {
            try
            {
                var appSettingFilename = "appsettings.json";
                var classPath = ClassPathHelper.GatewayAppSettingsClassPath(solutionDirectory, $"{appSettingFilename}", gatewayProjectName);

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

        private static string GetAppSettingsText(EnvironmentGateway env, List<Microservice> microservices)
        {
            var serilogSettings = GetSerilogSettings(env.EnvironmentName);
            var gatewayRoute = "";
            env.GatewayResources.ForEach(template => gatewayRoute += GetGatewayRoutes(template, microservices));

            if (env.EnvironmentName == "Development")
                return @$"{{
  ""AllowedHosts"": ""*"",
  ""GlobalConfiguration"": {{
    ""BaseUrl"": ""https://localhost:5050""
  }},
  ""Routes"": [{gatewayRoute}]
}}";
            else
                return @$"{{
  ""AllowedHosts"": ""*"",
  ""GlobalConfiguration"": {{
    ""BaseUrl"": ""https://localhost:5050""
  }},
  ""Routes"": [{gatewayRoute}]
}}";
        }

        private static string GetSerilogSettings(string env)
        {
            var writeTo = env == "Development" ? $@"
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
  ""AllowedHosts"": ""*"",
  ""GlobalConfiguration"": {{
    ""BaseUrl"": ""https://localhost:5050""
  }},
  ""Routes"": [
    {{
      ""DownstreamPathTemplate"": ""/api/recipes"",
      ""DownstreamScheme"": ""https"",
      ""DownstreamHostAndPorts"": [
        {{
          ""Host"": ""localhost"",
          ""Port"": 5467
        }}
      ],
      ""UpstreamPathTemplate"": ""/recipes"",
      ""UpstreamHttpMethod"": [ ""GET"" ]
    }}
  ]
}}";
        }

        private static string GetGatewayRoutes(GatewayResource template, List<Microservice> microservices)
        {
            var upstreamPathTemplate = template.GatewayRoute.StartsWith("/") ? template.GatewayRoute : @$"/{template.GatewayRoute}";

            // man this is ugly 🙈
            var microservice = microservices.Where(m => m.Entities.Any(e => String.Equals(e.Name, template.DownstreamEntityName, StringComparison.InvariantCultureIgnoreCase))).FirstOrDefault();
            if (microservice == null)
                throw new EntityNotFoundException(template.DownstreamEntityName);
            
            var entity = microservice.Entities.FirstOrDefault();

            var endpointBase = Utilities.EndpointBaseGenerator(entity.Plural);
            endpointBase = endpointBase.StartsWith("/") ? endpointBase : @$"/{endpointBase}";

            return $@"    {{
      ""UpstreamPathTemplate"": ""{upstreamPathTemplate}"",
      ""UpstreamHttpMethod"": [],
      ""DownstreamPathTemplate"": ""{endpointBase}"",
      ""DownstreamScheme"": ""https"",
      ""DownstreamHostAndPorts"": [
        {{
            ""Host"": ""localhost"",
            ""Port"": {microservice.Port}
         }}
      ]
    }}";
        }
    }
}
