namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using FluentAssertions.Common;
    using System;
    using System.IO;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class SwaggerBuilder
    {
        public static void AddSwagger(string solutionDirectory, ApiTemplate template)
        {
            if(!template.SwaggerConfig.IsSameOrEqualTo(new SwaggerConfig()))
            {
                AddSwaggerServiceExtension(solutionDirectory, template);
                AddSwaggerAppExtension(solutionDirectory, template);
                UpdateWebApiCsProjSwaggerSettings(solutionDirectory, template.SolutionName);
            }
        }

        public static void RegisterSwaggerInStartup(string solutionDirectory, ApiEnvironment env)
        {
            try
            {
                var classPath = ClassPathHelper.StartupClassPath(solutionDirectory, $"{Utilities.GetStartupName(env.EnvironmentName)}.cs");

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
                            if (line.Contains("#region Dynamic Services"))
                            {
                                newText += $"{Environment.NewLine}            services.AddSwaggerExtension();";
                            }
                            else if (line.Contains("#region Dynamic App"))
                            {
                                newText += $"{Environment.NewLine}            app.UseSwaggerExtension();";
                            }

                            output.WriteLine(newText);
                        }
                    }
                }

                // delete the old file and set the name of the new one to the original name
                File.Delete(classPath.FullClassPath);
                File.Move(tempPath, classPath.FullClassPath);

                GlobalSingleton.AddUpdatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
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

        private static void AddSwaggerServiceExtension(string solutionDirectory, ApiTemplate template)
        {
            try
            {
                var classPath = ClassPathHelper.WebApiExtensionsClassPath(solutionDirectory, $"ServiceExtensions.cs");

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
                            if (line.Contains("#region Swagger Region"))
                            {
                                newText += GetSwaggerServiceExtensionText(template);
                            }

                            output.WriteLine(newText);
                        }
                    }
                }

                // delete the old file and set the name of the new one to the original name
                File.Delete(classPath.FullClassPath);
                File.Move(tempPath, classPath.FullClassPath);

                GlobalSingleton.AddUpdatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
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

        private static string GetSwaggerServiceExtensionText(ApiTemplate template)
        {
            var contactUrlLine = IsCleanUri(template.SwaggerConfig.ApiContact.Url) 
                ? $@"Url = new Uri(""{ template.SwaggerConfig.ApiContact.Url }""),"
                : "";

            var LicenseUrlLine = IsCleanUri(template.SwaggerConfig.LicenseUrl)
                ? $@"Url = new Uri(""{ template.SwaggerConfig.LicenseUrl }""),"
                : "";

            var licenseText = GetLicenseText(template.SwaggerConfig.LicenseName, LicenseUrlLine);

            var SwaggerXmlComments = "";
            if (template.SwaggerConfig.AddSwaggerComments)
                SwaggerXmlComments = $@"

                    config.IncludeXmlComments(string.Format(@""{{0}}\{template.SolutionName}.WebApi.xml"", AppDomain.CurrentDomain.BaseDirectory));";

            var swaggerText = $@"
            public static void AddSwaggerExtension(this IServiceCollection services)
            {{
                services.AddSwaggerGen(config =>
                {{
                    config.SwaggerDoc(
                        ""v1"", 
                        new OpenApiInfo
                        {{
                            Version = ""v1"",
                            Title = ""{template.SwaggerConfig.Title}"",
                            Description = ""{template.SwaggerConfig.Description}"",
                            Contact = new OpenApiContact
                            {{
                                Name = ""{template.SwaggerConfig.ApiContact.Name}"",
                                Email = ""{template.SwaggerConfig.ApiContact.Email}"",
                                {contactUrlLine}
                            }},{licenseText}
                        }});{SwaggerXmlComments}
                }});
            }}";

            return swaggerText;
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

        private static void AddSwaggerAppExtension(string solutionDirectory, ApiTemplate template)
        {
            try
            {
                var classPath = ClassPathHelper.WebApiExtensionsClassPath(solutionDirectory, $"AppExtensions.cs");

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
                            if (line.Contains("#region Swagger Region"))
                            {
                                newText += GetSwaggerAppExtensionText(template);
                            }

                            output.WriteLine(newText);
                        }
                    }
                }

                // delete the old file and set the name of the new one to the original name
                File.Delete(classPath.FullClassPath);
                File.Move(tempPath, classPath.FullClassPath);

                GlobalSingleton.AddUpdatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
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
        private static bool IsCleanUri(string uri)
        {
            return Uri.TryCreate(uri, UriKind.Absolute, out var outUri) && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps);
        }

        private static string GetSwaggerAppExtensionText(ApiTemplate template)
        {
           var swaggerText = $@"
        public static void UseSwaggerExtension(this IApplicationBuilder app)
        {{
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {{
                c.SwaggerEndpoint(""{template.SwaggerConfig.SwaggerEndpointUrl}"", ""{template.SwaggerConfig.SwaggerEndpointName}"");
            }});
        }}";

            return swaggerText;
        }

        public static void UpdateWebApiCsProjSwaggerSettings(string solutionDirectory, string solutionName)
        {
            var classPath = ClassPathHelper.WebApiCsProjClassPath(solutionDirectory);

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
                            newText = @$"    <DocumentationFile>{solutionName}.WebApi.xml</DocumentationFile>";
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

            GlobalSingleton.AddUpdatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
        }
    }
}
