namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using FluentAssertions.Common;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class SwaggerBuilder
    {
        public static void AddSwagger(string solutionDirectory, SwaggerConfig swaggerConfig, string solutionName, bool addJwtAuthentication, List<Policy> policies, string projectBaseName, IFileSystem fileSystem)
        {
            if (!swaggerConfig.IsSameOrEqualTo(new SwaggerConfig()))
            {
                AddSwaggerServiceExtension(solutionDirectory, swaggerConfig, addJwtAuthentication, policies, projectBaseName);
                WebApiAppExtensionsBuilder.CreateSwaggerWebApiAppExtension(solutionDirectory, swaggerConfig, addJwtAuthentication, projectBaseName, fileSystem);
                UpdateWebApiCsProjSwaggerSettings(solutionDirectory, projectBaseName);
            }
        }

        public static void RegisterSwaggerInStartup(string solutionDirectory, ApiEnvironment env, string projectBaseName = "")
        {
            var classPath = ClassPathHelper.StartupClassPath(solutionDirectory, $"{Utilities.GetStartupName(env.EnvironmentName)}.cs", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

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
                        if (line.Contains("Dynamic Services"))
                        {
                            newText += $"{Environment.NewLine}            services.AddSwaggerExtension(_config);";
                        }
                        else if (line.Contains("Dynamic App"))
                        {
                            newText += $"{Environment.NewLine}            app.UseSwaggerExtension(_config);";
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);
        }

        private static void AddSwaggerServiceExtension(string solutionDirectory, SwaggerConfig swaggerConfig, bool addJwtAuthentication, List<Policy> policies, string projectBaseName)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(solutionDirectory, $"ServiceExtensions.cs", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

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
                        if (line.Contains("// Swagger Marker - Do Not Delete"))
                        {
                            newText += GetSwaggerServiceExtensionText(swaggerConfig, projectBaseName, addJwtAuthentication, policies);
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);
        }

        private static string GetSwaggerServiceExtensionText(SwaggerConfig swaggerConfig, string solutionName, bool addJwtAuthentication, List<Policy> policies)
        {
            var contactUrlLine = IsCleanUri(swaggerConfig.ApiContact.Url)
                ? $@"
                            Url = new Uri(""{ swaggerConfig.ApiContact.Url }""),"
                : "";

            var LicenseUrlLine = IsCleanUri(swaggerConfig.LicenseUrl)
                ? $@"Url = new Uri(""{ swaggerConfig.LicenseUrl }""),"
                : "";

            var licenseText = GetLicenseText(swaggerConfig.LicenseName, LicenseUrlLine);

            var policyScopes = GetPolicies(policies);
            var swaggerAuth = addJwtAuthentication ? $@"

                config.AddSecurityDefinition(""oauth2"", new OpenApiSecurityScheme
                {{
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {{
                        AuthorizationCode = new OpenApiOAuthFlow
                        {{
                            AuthorizationUrl = new Uri(configuration[""JwtSettings:AuthorizationUrl""]),
                            TokenUrl = new Uri(configuration[""JwtSettings:TokenUrl""]),
                            Scopes = new Dictionary<string, string>
                            {{{policyScopes}
                            }}
                        }}
                    }}
                }});

                config.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {{
                    {{
                        new OpenApiSecurityScheme
                        {{
                            Reference = new OpenApiReference
                            {{
                                Type = ReferenceType.SecurityScheme,
                                Id = ""oauth2""
                            }},
                            Scheme = ""oauth2"",
                            Name = ""oauth2"",
                            In = ParameterLocation.Header
                        }},
                        new List<string>()
                    }}
                }}); " : $@"";

            var SwaggerXmlComments = "";
            if (swaggerConfig.AddSwaggerComments)
                SwaggerXmlComments = $@"

                config.IncludeXmlComments(string.Format(@$""{{AppDomain.CurrentDomain.BaseDirectory}}{{Path.DirectorySeparatorChar}}{solutionName}.WebApi.xml""));";

            var swaggerText = $@"
        public static void AddSwaggerExtension(this IServiceCollection services, IConfiguration configuration)
        {{
            services.AddSwaggerGen(config =>
            {{
                config.SwaggerDoc(
                    ""v1"",
                    new OpenApiInfo
                    {{
                        Version = ""v1"",
                        Title = ""{swaggerConfig.Title}"",
                        Description = ""{swaggerConfig.Description}"",
                        Contact = new OpenApiContact
                        {{
                            Name = ""{swaggerConfig.ApiContact.Name}"",
                            Email = ""{swaggerConfig.ApiContact.Email}"",{contactUrlLine}
                        }},{licenseText}
                    }});{swaggerAuth}{SwaggerXmlComments}
            }});
        }}";

            return swaggerText;
        }

        private static object GetPolicies(List<Policy> policies)
        {
            var policyStrings = "";
            foreach (var policy in policies)
            {
                policyStrings += $@"
                                    {{ ""{policy.PolicyValue}"",""{policy.Name}"" }},";
            }

            return policyStrings;
        }

        private static string GetLicenseText(string licenseName, string licenseUrlLine)
        {
            if (licenseName?.Length > 0 || licenseUrlLine?.Length > 0)
                return $@"
                            License = new OpenApiLicense()
                            {{
                                Name = ""{licenseName}"",
                                Url = ""{licenseUrlLine}"",
                            }}";
            return "";
        }

        private static bool IsCleanUri(string uri)
        {
            return Uri.TryCreate(uri, UriKind.Absolute, out var outUri) && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps);
        }

        public static void UpdateWebApiCsProjSwaggerSettings(string solutionDirectory, string projectBaseName)
        {
            var classPath = ClassPathHelper.WebApiProjectClassPath(solutionDirectory, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

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
                        if (line.Contains($"DocumentationFile"))
                        {
                            newText = @$"    <DocumentationFile>{projectBaseName}.WebApi.xml</DocumentationFile>";
                        }
                        else if (line.Contains($"NoWarn"))
                        {
                            newText = newText.Replace("</NoWarn>", "1591;</NoWarn>");
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);
        }
    }
}