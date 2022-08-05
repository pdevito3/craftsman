namespace Craftsman.Builders;

using System;
using System.IO;
using Domain;
using FluentAssertions.Common;
using Helpers;
using Humanizer;
using Services;

public class SwaggerBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public SwaggerBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void AddSwagger(string solutionDirectory, SwaggerConfig swaggerConfig, string projectName, bool addJwtAuthentication, string policyName, string projectBaseName)
    {
        if (swaggerConfig.IsSameOrEqualTo(new SwaggerConfig())) return;

        AddSwaggerServiceExtension(solutionDirectory, projectBaseName, swaggerConfig, projectName, addJwtAuthentication, policyName);
        new WebApiAppExtensionsBuilder(_utilities).CreateSwaggerWebApiAppExtension(solutionDirectory, swaggerConfig, addJwtAuthentication, projectBaseName);
        UpdateWebApiCsProjSwaggerSettings(solutionDirectory, projectBaseName);
    }

    public void AddSwaggerServiceExtension(string srcDirectory, string projectBaseName, SwaggerConfig swaggerConfig, string projectName, bool addJwtAuthentication, string policyName)
    {
        var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"{FileNames.GetSwaggerServiceExtensionName()}.cs", projectBaseName);
        var fileText = GetSwaggerServiceExtensionText(classPath.ClassNamespace, swaggerConfig, projectName, addJwtAuthentication, policyName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetSwaggerServiceExtensionText(string classNamespace, SwaggerConfig swaggerConfig, string projectName, bool addJwtAuthentication, string policyName)
    {
        return @$"namespace {classNamespace};

using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

public static class SwaggerServiceExtension
{{
    {GetSwaggerServiceExtensionText(swaggerConfig, projectName, addJwtAuthentication, policyName)}
}}";
    }

    private static string GetSwaggerServiceExtensionText(SwaggerConfig swaggerConfig, string projectName, bool addJwtAuthentication, string policyName)
    {
        var contactUrlLine = IsCleanUri(swaggerConfig.ApiContact.Url)
            ? $@"
                            Url = new Uri(""{ swaggerConfig.ApiContact.Url }""),"
            : "";

        var licenseUrlLine = IsCleanUri(swaggerConfig.LicenseUrl)
            ? $@"Url = new Uri(""{ swaggerConfig.LicenseUrl }""),"
            : "";

        var licenseText = GetLicenseText(swaggerConfig.LicenseName, licenseUrlLine);

        // {{""{policyName}"", ""{projectName.Humanize()} access""}}
        var swaggerAuth = addJwtAuthentication ? $@"

            config.AddSecurityDefinition(""oauth2"", new OpenApiSecurityScheme
            {{
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {{
                    AuthorizationCode = new OpenApiOAuthFlow
                    {{
                        AuthorizationUrl = new Uri(Environment.GetEnvironmentVariable(""AUTH_AUTHORIZATION_URL"")),
                        TokenUrl = new Uri(Environment.GetEnvironmentVariable(""AUTH_TOKEN_URL"")),
                        Scopes = new Dictionary<string, string>
                        {{
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

        var swaggerText = $@"public static void AddSwaggerExtension(this IServiceCollection services)
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
                }});{swaggerAuth}{swaggerXmlComments}
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
