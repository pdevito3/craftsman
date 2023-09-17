namespace Craftsman.Builders.Tests.Utilities;

using Craftsman.Helpers;
using Craftsman.Services;
using MediatR;

public static class TestUsingsBuilder
{
    public enum TestingTarget
    {
        Unit,
        Integration,
        Functional
    }
    
    public record Command(TestingTarget TestingTarget) : IRequest;

    public class Handler : IRequestHandler<Command>
    {
        private readonly ICraftsmanUtilities _utilities;
        private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

        public Handler(ICraftsmanUtilities utilities,
            IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        {
            _utilities = utilities;
            _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        }

        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = GetTestingClassPath(request.TestingTarget);
            var fileText = GetFileText();
            _utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }

        private string GetFileText()
        {
            return @$"global using Xunit;
global using FluentAssertions;";
        }
        
        private ClassPath GetTestingClassPath(TestingTarget testingTarget)
        {
            return testingTarget switch
            {
                TestingTarget.Unit => ClassPathHelper.UnitTestProjectRootClassPath(
                    _scaffoldingDirectoryStore.TestDirectory, "Usings.cs", _scaffoldingDirectoryStore.ProjectBaseName),
                TestingTarget.Integration => ClassPathHelper.IntegrationTestProjectRootClassPath(
                    _scaffoldingDirectoryStore.TestDirectory, "Usings.cs", _scaffoldingDirectoryStore.ProjectBaseName),
                TestingTarget.Functional => ClassPathHelper.FunctionalTestProjectRootClassPath(
                    _scaffoldingDirectoryStore.TestDirectory, "Usings.cs", _scaffoldingDirectoryStore.ProjectBaseName),
                _ => throw new ArgumentOutOfRangeException(nameof(testingTarget), testingTarget, null)
            };
        }
    }
}