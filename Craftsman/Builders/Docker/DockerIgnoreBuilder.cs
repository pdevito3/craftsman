namespace Craftsman.Builders.Docker;

using Helpers;
using Services;
using MediatR;

public static class DockerIgnoreBuilder
{
    public sealed record Command : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.WebApiProjectRootClassPath(scaffoldingDirectoryStore.SrcDirectory, $".dockerignore", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetDockerIgnoreText();
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string GetDockerIgnoreText()
    {
        return @$"**/.dockerignore
**/.env
**/.git
**/.gitignore
**/.project
**/.settings
**/.toolstarget
**/.vs
**/.vscode
**/.idea
**/*.*proj.user
**/*.dbmdl
**/*.jfm
**/azds.yaml
**/bin
**/charts
**/docker-compose*
**/Dockerfile*
**/node_modules
**/npm-debug.log
**/obj
**/secrets.dev.yaml
**/values.dev.yaml
LICENSE
README.md";
    }
}