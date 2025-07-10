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

    public class Handler(ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = GetTestingClassPath(request.TestingTarget);
            var fileText = GetFileText();
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }

        private string GetFileText()
        {
            var exceptionClassPath = ClassPathHelper.ExceptionsClassPath(scaffoldingDirectoryStore.SrcDirectory, 
                "", 
                scaffoldingDirectoryStore.ProjectBaseName);
            return @$"global using Xunit;
global using FluentAssertions;
global using {exceptionClassPath.ClassNamespace};";
        }
        
        private ClassPath GetTestingClassPath(TestingTarget testingTarget)
        {
            return testingTarget switch
            {
                TestingTarget.Unit => ClassPathHelper.UnitTestProjectRootClassPath(
                    scaffoldingDirectoryStore.TestDirectory, "Usings.cs", scaffoldingDirectoryStore.ProjectBaseName),
                TestingTarget.Integration => ClassPathHelper.IntegrationTestProjectRootClassPath(
                    scaffoldingDirectoryStore.TestDirectory, "Usings.cs", scaffoldingDirectoryStore.ProjectBaseName),
                TestingTarget.Functional => ClassPathHelper.FunctionalTestProjectRootClassPath(
                    scaffoldingDirectoryStore.TestDirectory, "Usings.cs", scaffoldingDirectoryStore.ProjectBaseName),
                _ => throw new ArgumentOutOfRangeException(nameof(testingTarget), testingTarget, null)
            };
        }
    }
}