namespace Craftsman.Builders.Projects
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class FunctionalTestsCsProjBuilder
    {
        public static void CreateTestsCsProj(string solutionDirectory, string projectBaseName, bool addJwtAuth)
        {
            try
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

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        public static string GetTestsCsProjFileText(bool addJwtAuth, string solutionDirectory, string projectBaseName)
        {
            var coreClassPath = ClassPathHelper.CoreProjectClassPath(solutionDirectory, projectBaseName);
            var webApiClassPath = ClassPathHelper.WebApiProjectClassPath(solutionDirectory, projectBaseName);
            var infraClassPath = ClassPathHelper.InfrastructureProjectClassPath(solutionDirectory, projectBaseName);
            var sharedTestClassPath = ClassPathHelper.SharedTestProjectClassPath(solutionDirectory, projectBaseName);

            var authPackages = addJwtAuth ? @$"
    <PackageReference Include=""WebMotions.Fake.Authentication.JwtBearer"" Version=""3.1.0"" />" : "";

            return @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>

    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""AutoBogus"" Version=""2.12.0"" />
    <PackageReference Include=""Bogus"" Version=""32.0.2"" />
    <PackageReference Include=""Docker.DotNet"" Version=""3.125.4"" />
    <PackageReference Include=""FluentAssertions"" Version=""5.10.3"" />
    <PackageReference Include=""Microsoft.AspNetCore.Mvc.Testing"" Version=""5.0.1"" />
    <PackageReference Include=""MediatR"" Version=""9.0.0"" />
    <PackageReference Include=""Moq"" Version=""4.16.1"" />
    <PackageReference Include=""Newtonsoft.Json"" Version=""12.0.3"" />
    <PackageReference Include=""NUnit"" Version=""3.12.0"" />
    <PackageReference Include=""NUnit3TestAdapter"" Version=""3.16.1"" />
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""16.8.3"" />
    <PackageReference Include=""Respawn"" Version=""3.3.0"" />{authPackages}
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\src\{coreClassPath.ClassNamespace}\{coreClassPath.ClassName}"" />
    <ProjectReference Include=""..\..\src\{infraClassPath.ClassNamespace}\{infraClassPath.ClassName}"" />
    <ProjectReference Include=""..\..\src\{webApiClassPath.ClassNamespace}\{webApiClassPath.ClassName}"" />
    <ProjectReference Include=""..\{sharedTestClassPath.ClassNamespace}\{sharedTestClassPath.ClassName}"" />
  </ItemGroup>

</Project>";
        }
    }
}
