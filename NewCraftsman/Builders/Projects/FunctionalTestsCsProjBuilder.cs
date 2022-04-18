namespace NewCraftsman.Builders.Projects
{
    using System.IO;
    using System.Text;
    using Exceptions;

    public class FunctionalTestsCsProjBuilder
    {
        public static void CreateTestsCsProj(string solutionDirectory, string projectBaseName, bool addJwtAuth)
        {
            var classPath = ClassPathHelper.FunctionalTestProjectClassPath(solutionDirectory, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = "";
                data = GetTestsCsProjFileText(addJwtAuth, solutionDirectory, projectBaseName);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static string GetTestsCsProjFileText(bool addJwtAuth, string solutionDirectory, string projectBaseName)
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
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""17.0.0"" />
    <PackageReference Include=""Respawn"" Version=""4.0.0"" />
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
