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

    public void CreateSwaggerWebApiAppExtension(string srcDirectory, SwaggerConfig swaggerConfig, bool addJwtAuthentication, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiApplicationExtensionsClassPath(srcDirectory, $"SwaggerAppExtension.cs", projectBaseName);
        var fileText = GetSwaggerAppExtensionText(classPath.ClassNamespace, srcDirectory, swaggerConfig, addJwtAuthentication, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetSwaggerAppExtensionText(string classNamespace, string srcDirectory, SwaggerConfig swaggerConfig, bool addJwtAuthentication, string projectBaseName)
    {
        var webApiClassPath = ClassPathHelper.WebApiMiddlewareClassPath(srcDirectory, "", projectBaseName);
        var envServiceClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        return @$"namespace {classNamespace};

using Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Resources;
using Swashbuckle.AspNetCore.SwaggerUI;

public static class SwaggerAppExtension
{{
    {GetSwaggerAppExtensionText(addJwtAuthentication)}
}}";
    }

    private static string GetSwaggerAppExtensionText(bool addJwtAuthentication)
    {
        var swaggerAuth = addJwtAuthentication ? $@"
                var authOptions = configuration.GetAuthOptions();
                config.OAuthClientId(authOptions.ClientId);
                config.OAuthClientSecret(authOptions.ClientSecret);
                config.OAuthUsePkce();" : "";

        var swaggerText = $@"public static void UseSwaggerExtension(this WebApplication app, IConfiguration configuration, IWebHostEnvironment env)
    {{
        if (!env.IsEnvironment(Consts.Testing.FunctionalTestingEnvName))
        {{
            app.UseSwagger();
            app.UseSwaggerUI(
            config =>
            {{
                var descriptions = app.DescribeApiVersions();
                foreach (var description in descriptions)
                {{
                    var url = $""/swagger/{{description.GroupName}}/swagger.json"";
                    var name = description.GroupName.ToUpperInvariant();
                    config.SwaggerEndpoint(url, name);
                }}
                config.DocExpansion(DocExpansion.None);{swaggerAuth}
            }});
        }}
    }}";

        return swaggerText;
    }
}
