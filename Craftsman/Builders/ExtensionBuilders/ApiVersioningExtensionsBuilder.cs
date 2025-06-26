namespace Craftsman.Builders.ExtensionBuilders;

using Helpers;
using MediatR;
using Services;

public static class ApiVersioningExtensionsBuilder
{
    public sealed record ApiVersioningExtensionsBuilderCommand : IRequest;

    public class Handler(ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<ApiVersioningExtensionsBuilderCommand>
    {
        public Task Handle(ApiVersioningExtensionsBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(scaffoldingDirectoryStore.SrcDirectory, $"ApiVersioningServiceExtension.cs", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetApiVersioningServiceExtensionText(classPath.ClassNamespace);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
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