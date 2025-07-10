namespace Craftsman.Builders.Projects;

using Helpers;
using Services;
using MediatR;

public static class SharedKernelCsProjBuilder
{
    public sealed record Command : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.SharedKernelProjectClassPath(scaffoldingDirectoryStore.SolutionDirectory);
            var fileText = GetMessagesCsProjFileText();
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    public static string GetMessagesCsProjFileText()
    {
        return @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
  </PropertyGroup>

</Project>";
    }
}
