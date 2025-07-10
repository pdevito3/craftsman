namespace Craftsman.Builders.Tests.Utilities;

using Helpers;
using Services;
using MediatR;

public static class ApiRoutesBuilder
{
    public sealed record Command : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(scaffoldingDirectoryStore.TestDirectory, scaffoldingDirectoryStore.ProjectBaseName, "ApiRoutes.cs");
            var fileText = GetBaseText(classPath.ClassNamespace);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string GetBaseText(string classNamespace)
    {
        return @$"namespace {classNamespace};
public class ApiRoutes
{{
    public const string Base = ""api"";
    public const string Health = Base + ""/health"";

    // new api route marker - do not delete
}}";
    }
}