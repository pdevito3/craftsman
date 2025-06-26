namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class WebApiLaunchSettingsBuilder
{
    public sealed record WebApiLaunchSettingsBuilderCommand : IRequest;

    public class Handler(ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<WebApiLaunchSettingsBuilderCommand>
    {
        public Task Handle(WebApiLaunchSettingsBuilderCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.WebApiLaunchSettingsClassPath(scaffoldingDirectoryStore.SrcDirectory, $"launchSettings.json", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetLaunchSettingsText();
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }

        public static string GetLaunchSettingsText()
    {
        return @$"{{
  ""profiles"": {{
  }}
}}";
        }
    }
}
