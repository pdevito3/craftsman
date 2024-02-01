namespace Craftsman.Builders.ExtensionBuilders;

using Helpers;
using Services;

public class ApiVersioningExtensionsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public ApiVersioningExtensionsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateApiVersioningServiceExtension(string solutionDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(solutionDirectory, $"ApiVersioningServiceExtension.cs", projectBaseName);
        var fileText = GetApiVersioningServiceExtensionText(classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetApiVersioningServiceExtensionText(string classNamespace)
    {
        return @$"namespace {classNamespace};

using Asp.Versioning;
using Asp.Versioning.Conventions;

public static class ApiVersioningServiceExtension
{{
    public static void AddApiVersioningExtension(this IServiceCollection services)
    {{
        services.AddApiVersioning(static options =>
            {{
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            }})
            .AddMvc(static options =>
            {{
                options.Conventions.Add(new VersionByNamespaceConvention());
            }})
            .AddApiExplorer(
                static options =>
                {{
                    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // note: the specified format code will format the version as ""'v'major[.minor][-status]""
                    options.GroupNameFormat = ""'v'VVV"";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                }});
    }}
}}";
    }
}