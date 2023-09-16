namespace Craftsman.Builders.Projects;

using Helpers;
using Services;

public class UnitTestsCsProjBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public UnitTestsCsProjBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTestsCsProj(string solutionDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.UnitTestProjectClassPath(solutionDirectory, projectBaseName);
        _utilities.CreateFile(classPath, GetTestsCsProjFileText(solutionDirectory, projectBaseName));
    }

    public static string GetTestsCsProjFileText(string solutionDirectory, string projectBaseName)
    {
        var apiClassPath = ClassPathHelper.WebApiProjectClassPath(solutionDirectory, projectBaseName);
        var sharedTestClassPath = ClassPathHelper.SharedTestProjectClassPath(solutionDirectory, projectBaseName);

        return @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>

    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.AspNetCore.Mvc.Testing"" Version=""7.0.2"" />
    <PackageReference Include=""AutoBogusLifesupport"" Version=""2.14.0"" />
    <PackageReference Include=""Bogus"" Version=""34.0.2"" />
    <PackageReference Include=""FluentAssertions"" Version=""6.9.0"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore.Relational"" Version=""7.0.2"" />
    <PackageReference Include=""MockQueryable.Moq"" Version=""7.0.0"" />
    <PackageReference Include=""Moq"" Version=""4.18.4"" />
    <PackageReference Include=""NSubstitute"" Version=""5.1.0"" />
    <PackageReference Include=""NSubstitute.Analyzers.CSharp"" Version=""1.0.16"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""MockQueryable.NSubstitute"" Version=""7.0.0"" />
    <PackageReference Include=""xunit"" Version=""2.4.2"" />
    <PackageReference Include=""xunit.runner.visualstudio"" Version=""2.4.5"" />
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""17.4.1"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\src\{apiClassPath.ClassNamespace}\{apiClassPath.ClassName}"" />
    <ProjectReference Include=""..\{sharedTestClassPath.ClassNamespace}\{sharedTestClassPath.ClassName}"" />
  </ItemGroup>

</Project>";
    }
}
