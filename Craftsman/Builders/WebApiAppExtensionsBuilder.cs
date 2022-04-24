namespace Craftsman.Builders;

using Domain;
using Helpers;
using Services;

public class WebApiAppExtensionsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public WebApiAppExtensionsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateSwaggerWebApiAppExtension(string solutionDirectory, SwaggerConfig swaggerConfig, bool addJwtAuthentication, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiApplicationExtensionsClassPath(solutionDirectory, $"SwaggerAppExtension.cs", projectBaseName);
        var fileText = GetSwaggerAppExtensionText(classPath.ClassNamespace, solutionDirectory, swaggerConfig, addJwtAuthentication, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetSwaggerAppExtensionText(string classNamespace, string solutionDirectory, SwaggerConfig swaggerConfig, bool addJwtAuthentication, string projectBaseName)
    {
        var webApiClassPath = ClassPathHelper.WebApiMiddlewareClassPath(solutionDirectory, "", projectBaseName);
        return @$"namespace {classNamespace};

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.SwaggerUI;
using {webApiClassPath.ClassNamespace};

public static class SwaggerAppExtension
{{
    {GetSwaggerAppExtensionText(swaggerConfig, addJwtAuthentication)}
}}";
    }

    private static string GetSwaggerAppExtensionText(SwaggerConfig swaggerConfig, bool addJwtAuthentication)
    {
        var swaggerAuth = addJwtAuthentication ? $@"
            config.OAuthClientId(Environment.GetEnvironmentVariable(""AUTH_CLIENT_ID""));
            config.OAuthClientSecret(Environment.GetEnvironmentVariable(""AUTH_CLIENT_SECRET""));
            config.OAuthUsePkce();" : "";

        var swaggerText = $@"public static void UseSwaggerExtension(this IApplicationBuilder app, IConfiguration configuration)
    {{
        app.UseSwagger();
        app.UseSwaggerUI(config =>
        {{
            config.SwaggerEndpoint(""{swaggerConfig.SwaggerEndpointUrl}"", ""{swaggerConfig.SwaggerEndpointName}"");
            config.DocExpansion(DocExpansion.None);{swaggerAuth}
        }});
    }}";

        return swaggerText;
    }
}
