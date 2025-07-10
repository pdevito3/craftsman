namespace Craftsman.Builders.AuthServer;

using Craftsman.Helpers;
using Craftsman.Services;
using MediatR;

public static class PulumiYamlBuilders
{
    public sealed record CreateBaseFileCommand(string ProjectBaseName) : IRequest;

    public class CreateBaseFileHandler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<CreateBaseFileCommand>
    {
        public Task Handle(CreateBaseFileCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.AuthServerProjectRootClassPath(scaffoldingDirectoryStore.SolutionDirectory, "Pulumi.yaml", request.ProjectBaseName);
            var fileText = GetBaseFileText(request.ProjectBaseName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    public sealed record CreateDevConfigCommand(string ProjectBaseName, int? Port, string Username, string Password) : IRequest;

    public class CreateDevConfigHandler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<CreateDevConfigCommand>
    {
        public Task Handle(CreateDevConfigCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.AuthServerProjectRootClassPath(scaffoldingDirectoryStore.SolutionDirectory, "Pulumi.dev.yaml", request.ProjectBaseName);
            var fileText = GetDevConfigText(request.Port, request.Username, request.Password);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string GetBaseFileText(string projectBaseName)
    {
        return @$"name: {projectBaseName}
runtime: dotnet
description: Local setup for {projectBaseName}
";
    }

    private static string GetDevConfigText(int? port, string username, string password)
    {
        return @$"config:
  keycloak:url: http://localhost:{port}
  keycloak:clientId: admin-cli
  keycloak:username: {username}
  keycloak:password: {password}
";
    }
}
