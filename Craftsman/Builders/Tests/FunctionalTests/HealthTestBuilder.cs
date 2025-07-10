namespace Craftsman.Builders.Tests.FunctionalTests;

using System.IO;
using Helpers;
using Services;
using MediatR;

public static class HealthTestBuilder
{
    public sealed record Command : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.FunctionalTestClassPath(scaffoldingDirectoryStore.SolutionDirectory, $"HealthCheckTests.cs", "HealthChecks", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = WriteTestFileText(scaffoldingDirectoryStore.SolutionDirectory, classPath, scaffoldingDirectoryStore.ProjectBaseName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string WriteTestFileText(string solutionDirectory, ClassPath classPath, string projectBaseName)
    {
        var testUtilClassPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(solutionDirectory, projectBaseName, "");

        return @$"namespace {classPath.ClassNamespace};

using {testUtilClassPath.ClassNamespace};
using System.Net;
using System.Threading.Tasks;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
{{
    {HealthTest()}
}}";
    }

    private static string HealthTest()
    {
        return $@"[Fact]
    public async Task health_check_returns_ok()
    {{
        // Arrange
        // N/A

        // Act
        var result = await FactoryClient.GetRequestAsync(ApiRoutes.Health);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }}";
    }
}