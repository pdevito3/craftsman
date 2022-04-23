namespace Craftsman.Builders.Projects
{
    using Helpers;
    using Services;

    public class FunctionalTestsCsProjBuilder
    {
        private readonly ICraftsmanUtilities _utilities;

        public FunctionalTestsCsProjBuilder(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }

        public void CreateTestsCsProj(string solutionDirectory, string projectBaseName)
        {
            var classPath = ClassPathHelper.FunctionalTestProjectClassPath(solutionDirectory, projectBaseName);
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
    <PackageReference Include=""Microsoft.AspNetCore.Mvc.Testing"" Version=""6.0.4"" />
    <PackageReference Include=""AutoBogus"" Version=""2.13.1"" />
    <PackageReference Include=""Bogus"" Version=""34.0.2"" />
    <PackageReference Include=""Docker.DotNet"" Version=""3.125.5"" />
    <PackageReference Include=""FluentAssertions"" Version=""6.6.0"" />
    <PackageReference Include=""MediatR"" Version=""10.0.1"" />
    <PackageReference Include=""Moq"" Version=""4.17.2"" />
    <PackageReference Include=""NUnit"" Version=""3.13.3"" />
    <PackageReference Include=""NUnit3TestAdapter"" Version=""4.2.1"" />
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""17.1.0"" />
    <PackageReference Include=""Respawn"" Version=""5.0.1"" />
    <PackageReference Include=""WebMotions.Fake.Authentication.JwtBearer"" Version=""6.0.0"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\src\{webApiClassPath.ClassNamespace}\{webApiClassPath.ClassName}"" />
    <ProjectReference Include=""..\{sharedTestClassPath.ClassNamespace}\{sharedTestClassPath.ClassName}"" />
  </ItemGroup>

</Project>";
        }
    }
}
