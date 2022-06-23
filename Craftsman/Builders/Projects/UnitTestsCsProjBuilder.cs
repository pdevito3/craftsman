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
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>

    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.AspNetCore.Mvc.Testing"" Version=""6.0.0"" />
    <PackageReference Include=""AutoBogus"" Version=""2.13.1"" />
    <PackageReference Include=""Bogus"" Version=""34.0.2"" />
    <PackageReference Include=""FakeItEasy"" Version=""7.3.1"" />
    <PackageReference Include=""FakeItEasy.Analyzer.CSharp"" Version=""6.1.0"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""FluentAssertions"" Version=""6.6.0"" />
    <PackageReference Include=""MockQueryable.Moq"" Version=""6.0.1"" />
    <PackageReference Include=""Moq"" Version=""4.17.2"" />
    <PackageReference Include=""NSubstitute"" Version=""4.3.0"" />
    <PackageReference Include=""NUnit"" Version=""3.13.3"" />
    <PackageReference Include=""NUnit3TestAdapter"" Version=""4.2.1"" />
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""17.1.0"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\src\{apiClassPath.ClassNamespace}\{apiClassPath.ClassName}"" />
    <ProjectReference Include=""..\{sharedTestClassPath.ClassNamespace}\{sharedTestClassPath.ClassName}"" />
  </ItemGroup>

</Project>";
    }
}
