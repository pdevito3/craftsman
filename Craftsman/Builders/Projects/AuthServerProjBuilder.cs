namespace Craftsman.Builders.Projects;

using Helpers;
using Services;
using MediatR;

public static class AuthServerProjBuilder
{
    public sealed record Command(string ProjectBaseName) : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.WebApiProjectClassPath(scaffoldingDirectoryStore.SolutionDirectory, request.ProjectBaseName);
            var fileText = ProjectFileText();
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    public static string ProjectFileText()
    {
        return @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Pulumi.Keycloak"" Version=""5.3.5"" />
  </ItemGroup>

</Project>";
    }
}
