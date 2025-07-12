namespace Craftsman.Builders.Projects;

using Helpers;
using Services;
using MediatR;

public static class SharedTestsCsProjBuilder
{
    public sealed record Command : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.SharedTestProjectClassPath(scaffoldingDirectoryStore.SolutionDirectory, scaffoldingDirectoryStore.ProjectBaseName);
            utilities.CreateFile(classPath, GetTestsCsProjFileText(scaffoldingDirectoryStore.SolutionDirectory, scaffoldingDirectoryStore.ProjectBaseName));
            return Task.CompletedTask;
        }
    }

    public static string GetTestsCsProjFileText(string solutionDirectory, string projectBaseName)
    {
        var apiClassPath = ClassPathHelper.WebApiProjectClassPath(solutionDirectory, projectBaseName);

        // lang=xml
        return @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Ardalis.SmartEnum"" Version=""7.0.0"" />
    <PackageReference Include=""AutoBogusLifesupport"" Version=""2.14.0"" />
    <PackageReference Include=""Bogus"" Version=""34.0.2"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\src\{apiClassPath.ClassNamespace}\{apiClassPath.ClassName}"" />
  </ItemGroup>

</Project>";
    }
}
