namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class QueryKitConfigBuilder
{
    public sealed record QueryKitConfigBuilderCommand : IRequest;

    public class Handler(ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<QueryKitConfigBuilderCommand>
    {
        public Task Handle(QueryKitConfigBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.WebApiResourcesClassPath(scaffoldingDirectoryStore.SrcDirectory, "CustomQueryKitConfiguration.cs", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetConfigText(classPath.ClassNamespace);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }

        private static string GetConfigText(string classNamespace)
    {
        return @$"namespace {classNamespace};

using QueryKit.Configuration;

public class CustomQueryKitConfiguration : QueryKitConfiguration
{{
    public CustomQueryKitConfiguration(Action<QueryKitSettings>? configureSettings = null)
        : base(settings => 
        {{
            // configure custom global settings here
            // settings.EqualsOperator = ""eq"";

            configureSettings?.Invoke(settings);
        }})
    {{
    }}
}}";
        }
    }
}
