namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class EditorConfigBuilder
{
    public sealed record EditorConfigBuilderCommand : IRequest;

    public class Handler(ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<EditorConfigBuilderCommand>
    {
        public Task Handle(EditorConfigBuilderCommand request, CancellationToken cancellationToken)
        {
            var appSettingFilename = FileNames.GetAppSettingsName();
            var classPath = ClassPathHelper.WebApiEditorConfigClassPath(scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText();
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }

        private static string GetFileText()
        {
            return "[*.cs]\ndotnet_diagnostic.RMG012.severity = error # Unmapped or non-automappable target member for Mapperly\n";
        }
    }
}
