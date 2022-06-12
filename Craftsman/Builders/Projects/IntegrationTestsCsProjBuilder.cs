namespace Craftsman.Builders.Projects;

using Helpers;
using Services;

public class IntegrationTestsCsProjBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public IntegrationTestsCsProjBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTestsCsProj(string solutionDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.IntegrationTestProjectClassPath(solutionDirectory, projectBaseName);
        _utilities.CreateFile(classPath, GetTestsCsProjFileText(solutionDirectory, projectBaseName));
    }

    public static string GetTestsCsProjFileText(string solutionDirectory, string projectBaseName)
    {
        var webApiClassPath = ClassPathHelper.WebApiProjectClassPath(solutionDirectory, projectBaseName);
        var sharedTestClassPath = ClassPathHelper.SharedTestProjectClassPath(solutionDirectory, projectBaseName);

        return @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>

    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""AutoBogus"" Version=""2.13.1"" />
    <PackageReference Include=""Bogus"" Version=""34.0.2"" />
    <PackageReference Include=""Docker.DotNet"" Version=""3.125.5"" />
    <PackageReference Include=""DotNet.Testcontainers"" Version=""1.5.0"" />
    <PackageReference Include=""Ductus.FluentDocker"" Version=""2.10.45"" />
    <PackageReference Include=""FluentAssertions"" Version=""6.6.0"" />
    <PackageReference Include=""MediatR"" Version=""10.0.1"" />
    <PackageReference Include=""Microsoft.AspNetCore.Mvc.Testing"" Version=""6.0.4"" />
    <PackageReference Include=""Moq"" Version=""4.17.2"" />
    <PackageReference Include=""Npgsql"" Version=""6.0.4"" />
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""17.1.0"" />
    <PackageReference Include=""Respawn"" Version=""5.0.1"" />
    <PackageReference Include=""xunit"" Version=""2.4.1"" />
    <PackageReference Include=""xunit.runner.visualstudio"" Version=""2.4.5"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\src\{webApiClassPath.ClassNamespace}\{webApiClassPath.ClassName}"" />
    <ProjectReference Include=""..\{sharedTestClassPath.ClassNamespace}\{sharedTestClassPath.ClassName}"" />
  </ItemGroup>

</Project>";
    }
}
