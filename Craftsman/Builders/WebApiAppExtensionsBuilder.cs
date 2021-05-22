namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO.Abstractions;
    using System.Text;

    public class WebApiAppExtensionsBuilder
    {
        public static void CreateErrorHandlerWebApiAppExtension(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiApplicationExtensionsClassPath(solutionDirectory, $"ErrorHandlerAppExtension.cs", projectBaseName);

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (var fs = fileSystem.File.Create(classPath.FullClassPath))
            {
                var data = "";
                data = GetErrorHandlerAppExtensionText(classPath.ClassNamespace, solutionDirectory, projectBaseName);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static string GetErrorHandlerAppExtensionText(string classNamespace, string solutionDirectory, string solutionName)
        {
            var webApiClassPath = ClassPathHelper.WebApiMiddlewareClassPath(solutionDirectory, "", solutionName);
            return @$"namespace {classNamespace}
{{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using {webApiClassPath.ClassNamespace};

    public static class ErrorHandlerAppExtension
    {{
        public static void UseErrorHandlingMiddleware(this IApplicationBuilder app)
        {{
            app.UseMiddleware<ErrorHandlerMiddleware>();
        }}
    }}
}}";
        }

        public static void CreateSwaggerWebApiAppExtension(string solutionDirectory, SwaggerConfig swaggerConfig, bool addJwtAuthentication, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiApplicationExtensionsClassPath(solutionDirectory, $"SwaggerAppExtension.cs", projectBaseName);

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (var fs = fileSystem.File.Create(classPath.FullClassPath))
            {
                var data = "";
                data = GetSwaggerAppExtensionText(classPath.ClassNamespace, solutionDirectory, swaggerConfig, addJwtAuthentication, projectBaseName);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static string GetSwaggerAppExtensionText(string classNamespace, string solutionDirectory, SwaggerConfig swaggerConfig, bool addJwtAuthentication, string projectBaseName)
        {
            var webApiClassPath = ClassPathHelper.WebApiMiddlewareClassPath(solutionDirectory, "", projectBaseName);
            return @$"namespace {classNamespace}
{{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using {webApiClassPath.ClassNamespace};

    public static class SwaggerAppExtension
    {{
        {GetSwaggerAppExtensionText(swaggerConfig, addJwtAuthentication)}
    }}
}}";
        }

        private static string GetSwaggerAppExtensionText(SwaggerConfig swaggerConfig, bool addJwtAuthentication)
        {
            var swaggerAuth = addJwtAuthentication ? $@"
                config.OAuthClientId(configuration[""JwtSettings:ClientId""]);
                config.OAuthUsePkce();" : "";

            var swaggerText = $@"public static void UseSwaggerExtension(this IApplicationBuilder app, IConfiguration configuration)
        {{
            app.UseSwagger();
            app.UseSwaggerUI(config =>
            {{
                config.SwaggerEndpoint(""{swaggerConfig.SwaggerEndpointUrl}"", ""{swaggerConfig.SwaggerEndpointName}"");{swaggerAuth}
            }});
        }}";

            return swaggerText;
        }
    }
}