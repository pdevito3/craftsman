namespace Craftsman.Builders.Projects;

using Helpers;
using Services;

public class SharedTestsCsProjBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public SharedTestsCsProjBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTestsCsProj(string solutionDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.SharedTestProjectClassPath(solutionDirectory, projectBaseName);
        _utilities.CreateFile(classPath, GetTestsCsProjFileText(solutionDirectory, projectBaseName));
    }

    public static string GetTestsCsProjFileText(string solutionDirectory, string projectBaseName)
    {
        var apiClassPath = ClassPathHelper.WebApiProjectClassPath(solutionDirectory, projectBaseName);

        return @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Ardalis.SmartEnum"" Version=""7.0.0"" />
    <PackageReference Include=""AutoBogusLifesupport"" Version=""2.14.0"" />
    <PackageReference Include=""Bogus"" Version=""34.0.2"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore.Relational"" Version=""7.0.2"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\src\{apiClassPath.ClassNamespace}\{apiClassPath.ClassName}"" />
  </ItemGroup>

</Project>";
    }
}
