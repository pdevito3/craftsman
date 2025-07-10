namespace Craftsman.Builders;

using Domain;
using Helpers;
using Services;
using MediatR;

public static class WebApiAppExtensionsBuilder
{
    public sealed record Command(SwaggerConfig SwaggerConfig, bool AddJwtAuthentication) : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.WebApiApplicationExtensionsClassPath(scaffoldingDirectoryStore.SrcDirectory, $"SwaggerAppExtension.cs", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetSwaggerAppExtensionText(classPath.ClassNamespace, scaffoldingDirectoryStore.SrcDirectory, request.SwaggerConfig, request.AddJwtAuthentication, scaffoldingDirectoryStore.ProjectBaseName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    public static string GetSwaggerAppExtensionText(string classNamespace, string srcDirectory, SwaggerConfig swaggerConfig, bool addJwtAuthentication, string projectBaseName)
    {
        var webApiClassPath = ClassPathHelper.WebApiMiddlewareClassPath(srcDirectory, "", projectBaseName);
        var envServiceClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        return @$"namespace {classNamespace};

using Resources;
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