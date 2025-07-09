namespace Craftsman.Builders.AuthServer;

using Craftsman.Helpers;
using Craftsman.Services;
using MediatR;

public static class ProgramBuilder
{
    public sealed record Command(string AuthServerProjectName) : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.WebApiProjectRootClassPath(scaffoldingDirectoryStore.SolutionDirectory, $"Program.cs", request.AuthServerProjectName);
            var fileText = GetAuthServerProgramText(classPath.ClassNamespace);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    public static string GetAuthServerProgramText(string classNamespace)
    {
        return @$"namespace {classNamespace};

using System.Threading.Tasks;
using Pulumi;

internal static class Program
{{
    private static Task<int> Main() => Deployment.RunAsync<RealmBuild>();
}}";
    }
}
