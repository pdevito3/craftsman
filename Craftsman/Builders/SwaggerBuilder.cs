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
                RegisterSwaggerInStartup(solutionDirectory);
            }
        }

        private static void RegisterSwaggerInStartup(string solutionDirectory)
        {
            try
            {
                var classPath = ClassPathHelper.StartupClassPath(solutionDirectory, "Startup.cs");

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

                // delete the old file and set the name of the new one to the original nape
                File.Delete(classPath.FullClassPath);
                File.Move(tempPath, classPath.FullClassPath);

                GlobalSingleton.AddUpdatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}\\", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occured when running the API command.\nThe error details are: \n{e.Message}");
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

                // delete the old file and set the name of the new one to the original nape
                File.Delete(classPath.FullClassPath);
                File.Move(tempPath, classPath.FullClassPath);

                GlobalSingleton.AddUpdatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}\\", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occured when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        private static string GetSwaggerServiceExtensionText(ApiTemplate template)
        {
            var urlLine = "";
            if (Uri.TryCreate(template.SwaggerConfig.ApiContact.Url, UriKind.Absolute, out var outUri) && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps))
                urlLine = $@"Url = new Uri(""{ template.SwaggerConfig.ApiContact.Url }""),";

            var swaggerText = $@"
            public static void AddSwaggerExtension(this IServiceCollection services)
            {{
                services.AddSwaggerGen(config =>
                {{
                    config.SwaggerDoc(""v1"", new OpenApiInfo
                    {{
                        Version = ""v1"",
                        Title = ""{template.SwaggerConfig.Title}"",
                        Description = ""{template.SwaggerConfig.Description}"",
                        Contact = new OpenApiContact
                        {{
                            Name = ""{template.SwaggerConfig.ApiContact.Name}"",
                            Email = ""{template.SwaggerConfig.ApiContact.Email}"",
                            {urlLine}
                        }}
                    }});
                }});
            }}";

            var nswagText = @$"
            public static void AddSwaggerExtension(this IServiceCollection services)
            {{
                services.AddSwaggerDocument(config => {{
                    config.PostProcess = document =>
                    {{
                        document.Info.Version = ""v1"";
                        document.Info.Title = ""{template.SwaggerConfig.Title}"";
                        document.Info.Description = ""{template.SwaggerConfig.Description}"";
                        document.Info.Contact = new OpenApiContact
                        {{
                            Name = ""{template.SwaggerConfig.ApiContact.Name}"",
                            Email = ""{template.SwaggerConfig.ApiContact.Email}"",
                            Url = ""{template.SwaggerConfig.ApiContact.Url}"",
                        }};
                        document.Info.License = new OpenApiLicense()
                        {{
                            Name = $""Copyright {{DateTime.Now.Year}}"",
                        }};
                    }};
                }});
            }}";

            return nswagText;
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

                // delete the old file and set the name of the new one to the original nape
                File.Delete(classPath.FullClassPath);
                File.Move(tempPath, classPath.FullClassPath);

                GlobalSingleton.AddUpdatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}\\", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occured when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        private static string GetSwaggerAppExtensionText(ApiTemplate template)
        {
            var swaggerText = $@"
        public static void UseSwaggerExtension(this IApplicationBuilder app)
        {{
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {{
                c.SwaggerEndpoint(""{template.SwaggerConfig.SwaggerUi.Url}"", ""{template.SwaggerConfig.SwaggerUi.Title}"");
            }});
        }}";

            var nswagText = @$"            
        public static void UseSwaggerExtension(this IApplicationBuilder app)
        {{
            app.UseSwagger();
            app.UseSwaggerUi3();
        }}";

            return nswagText;
        }
    }
}
