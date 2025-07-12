namespace Craftsman.Builders.Projects;

using Domain;
using Helpers;
using Services;
using MediatR;

public static class FunctionalTestsCsProjBuilder
{
    public sealed record Command(DbProvider Provider) : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.FunctionalTestProjectClassPath(scaffoldingDirectoryStore.SolutionDirectory, scaffoldingDirectoryStore.ProjectBaseName);
            utilities.CreateFile(classPath, GetTestsCsProjFileText(scaffoldingDirectoryStore.SolutionDirectory, scaffoldingDirectoryStore.ProjectBaseName, request.Provider));
            return Task.CompletedTask;
        }
    }

    public static string GetTestsCsProjFileText(string solutionDirectory, string projectBaseName, DbProvider provider)
    {
        var webApiClassPath = ClassPathHelper.WebApiProjectClassPath(solutionDirectory, projectBaseName);
        var sharedTestClassPath = ClassPathHelper.SharedTestProjectClassPath(solutionDirectory, projectBaseName);

        // lang=xml
        return @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>

    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""AutoBogusLifesupport"" Version=""2.14.0"" />
    <PackageReference Include=""Bogus"" Version=""35.0.1"" />
    <PackageReference Include=""FluentAssertions"" Version=""6.12.0"" />
    <PackageReference Include=""MediatR"" Version=""12.2.0"" />
    <PackageReference Include=""Microsoft.AspNetCore.Mvc.Testing"" Version=""8.0.0"" />
    <PackageReference Include=""Microsoft.AspNetCore.Mvc.NewtonsoftJson"" Version=""8.0.0"" />
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""17.8.0"" />
    <PackageReference Include=""NSubstitute"" Version=""5.1.0"" />
    <PackageReference Include=""NSubstitute.Analyzers.CSharp"" Version=""1.0.16"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""WebMotions.Fake.Authentication.JwtBearer"" Version=""8.0.0"" />
    <PackageReference Include=""Testcontainers"" Version=""3.6.0"" />
    <PackageReference Include=""Testcontainers.RabbitMq"" Version=""3.6.0"" />{provider.TestingCsProjNugetPackages()}
    <PackageReference Include=""xunit"" Version=""2.6.4"" />
    <PackageReference Include=""xunit.runner.visualstudio"" Version=""2.5.6"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\src\{webApiClassPath.ClassNamespace}\{webApiClassPath.ClassName}"" />
    <ProjectReference Include=""..\{sharedTestClassPath.ClassNamespace}\{sharedTestClassPath.ClassName}"" />
  </ItemGroup>

</Project>";
    }
}
