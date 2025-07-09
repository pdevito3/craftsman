namespace Craftsman.Builders.AuthServer;

using Craftsman.Helpers;
using Craftsman.Services;
using MediatR;

public static class ScopeFactoryBuilder
{
    public sealed record Command(string ProjectBaseName) : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.AuthServerFactoriesClassPath(scaffoldingDirectoryStore.SolutionDirectory, "ScopeFactory.cs", request.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string GetFileText(string classNamespace)
    {
        return @$"namespace {classNamespace};

using Pulumi;
using Pulumi.Keycloak.OpenId;

public class ScopeFactory
{{
    public static ClientScope CreateScope(Output<string> realmId, string scopeName)
    {{
        return new ClientScope($""{{scopeName}}-scope"", new ClientScopeArgs()
        {{
            Name = scopeName,
            RealmId = realmId,
        }});
    }}
}}";
    }
}
