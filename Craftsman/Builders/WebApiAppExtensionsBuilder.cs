namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class WebApiAppExtensionsBuilder
    {
        public static void CreateWebApiAppExtension(string solutionDirectory, string solutionName, IFileSystem fileSystem)
        {
            try
            {
                var classPath = ClassPathHelper.WebApiExtensionsClassPath(solutionDirectory, $"AppExtensions.cs", solutionName);

                if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                    fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

                if (fileSystem.File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (var fs = fileSystem.File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetAppExtensionText(classPath.ClassNamespace, solutionDirectory, solutionName);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }
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

        public static string GetAppExtensionText(string classNamespace, string solutionDirectory, string solutionName)
        {
            var webApiClassPath = ClassPathHelper.WebApiMiddlewareClassPath(solutionDirectory, "", solutionName);
            return @$"namespace {classNamespace}
{{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using {webApiClassPath.ClassNamespace};

    public static class AppExtensions
    {{
        // Swagger Marker - Do Not Delete

        public static void UseErrorHandlingMiddleware(this IApplicationBuilder app)
        {{
            app.UseMiddleware<ErrorHandlerMiddleware>();
        }}
    }}
}}";
        }
    }
}
