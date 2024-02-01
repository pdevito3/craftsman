namespace Craftsman.Builders;

using System;
using System.IO;
using System.IO.Abstractions;
using Domain;
using Helpers;
using Humanizer;
using Services;

public class SwaggerBuilder
{
    private readonly ICraftsmanUtilities _utilities;
    private readonly IFileSystem _fileSystem;

    public SwaggerBuilder(ICraftsmanUtilities utilities, IFileSystem fileSystem)
    {
        _utilities = utilities;
        _fileSystem = fileSystem;
    }

    public void AddSwagger(string srcDirectory, SwaggerConfig swaggerConfig, string projectName, bool addJwtAuthentication, string audience, string projectBaseName)
    {
        if (swaggerConfig.Equals(new SwaggerConfig())) return;

        AddSwaggerServiceExtension(srcDirectory, projectBaseName, swaggerConfig, projectName, addJwtAuthentication, audience);
        new WebApiAppExtensionsBuilder(_utilities).CreateSwaggerWebApiAppExtension(srcDirectory, swaggerConfig, addJwtAuthentication, projectBaseName);
        UpdateWebApiCsProjSwaggerSettings(srcDirectory, projectBaseName);
    }

    public void AddSwaggerServiceExtension(string srcDirectory, string projectBaseName, SwaggerConfig swaggerConfig, string projectName, bool addJwtAuthentication, string audience)
    {
        var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"{FileNames.GetSwaggerServiceExtensionName()}.cs", projectBaseName);
        var fileText = GetSwaggerServiceExtensionText(classPath.ClassNamespace, swaggerConfig, projectName, addJwtAuthentication, audience, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetSwaggerServiceExtensionText(string classNamespace, SwaggerConfig swaggerConfig, string projectName, bool addJwtAuthentication, string audience, string srcDirectory, string projectBaseName)
    {
        var envServiceClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        return @$"namespace {classNamespace};

using {envServiceClassPath.ClassNamespace};
using Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Collections.Generic;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

{GetSwaggerServiceExtensionText(swaggerConfig, projectName, addJwtAuthentication, audience)}";
    }

    private static string GetSwaggerServiceExtensionText(SwaggerConfig swaggerConfig, string projectName, bool addJwtAuthentication, string audience)
    {
        var swaggerAuth = addJwtAuthentication ? $@"

            config.AddSecurityDefinition(""oauth2"", new OpenApiSecurityScheme
            {{
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {{
                    AuthorizationCode = new OpenApiOAuthFlow
                    {{
                        AuthorizationUrl = new Uri(authOptions.AuthorizationUrl),
                        TokenUrl = new Uri(authOptions.TokenUrl),
                        Scopes = new Dictionary<string, string>
                        {{
                            {{""{audience}"", ""{audience.Humanize().Titleize()} Access""}}
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

        var swaggerXmlComments = "";
        if (swaggerConfig.AddSwaggerComments)
            swaggerXmlComments = $@"

            config.IncludeXmlComments(string.Format(@$""{{AppDomain.CurrentDomain.BaseDirectory}}{{Path.DirectorySeparatorChar}}{projectName}.WebApi.xml""));";

        var swaggerText = $@"public static class SwaggerServiceExtension
{{
    public static void AddSwaggerExtension(this IServiceCollection services, 
        IConfiguration configuration)
    {{
        var authOptions = configuration.GetAuthOptions();
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen(config =>
        {{
            config.CustomSchemaIds(type => type.ToString().Replace(""+"", "".""));
            config.MapType<DateOnly>(() => new OpenApiSchema
            {{
                Type = ""string"",
                Format = ""date""
            }});{swaggerAuth}{swaggerXmlComments}
        }});
    }}
}}

public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
{{
    public void Configure(SwaggerGenOptions options)
    {{
        foreach (var description in provider.ApiVersionDescriptions)
        {{
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {{
                Version = description.ApiVersion.ToString(),
                Title = ""{swaggerConfig.Title}"",
                Description = ""{swaggerConfig.Description}""
            }});
        }}
    }}
}}
";

        return swaggerText;
    }

    private static bool IsCleanUri(string uri)
    {
        return Uri.TryCreate(uri, UriKind.Absolute, out var outUri) && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps);
    }

    public void UpdateWebApiCsProjSwaggerSettings(string solutionDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiProjectClassPath(solutionDirectory, projectBaseName);

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
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
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }
}
