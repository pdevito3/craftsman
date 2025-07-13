namespace Craftsman.Builders.Tests.Utilities;

using Helpers;
using Services;
using MediatR;

public static class IntegrationTestBaseBuilder
{
    public sealed record Command : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.IntegrationTestProjectRootClassPath(scaffoldingDirectoryStore.TestDirectory, "TestBase.cs", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetBaseText(classPath.ClassNamespace);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    public static string GetBaseText(string classNamespace)
    {
        return @$"namespace {classNamespace};

using AutoBogus;
using Xunit;

[Collection(nameof(TestFixture))]
public class TestBase : IDisposable
{{
    public TestBase()
    {{
        AutoFaker.Configure(builder =>
        {{
            // configure global autobogus settings here
            builder.WithDateTimeKind(DateTimeKind.Utc)
                .WithRecursiveDepth(3)
                .WithTreeDepth(1)
                .WithRepeatCount(1);
        }});
    }}
    
    public void Dispose()
    {{
    }}
}}";
    }
}