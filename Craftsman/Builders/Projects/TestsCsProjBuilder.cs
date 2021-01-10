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

    public class TestsCsProjBuilder
    {
        public static void CreateTestsCsProj(string solutionDirectory, string solutionName)
        {
            try
            {
                var classPath = ClassPathHelper.TestProjectClassPath(solutionDirectory, solutionName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetTestsCsProjFileText();
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

        public static string GetTestsCsProjFileText()
        {
            return @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>

    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""AutoBogus"" Version=""2.12.0"" />
    <PackageReference Include=""Bogus"" Version=""32.0.2"" />
    <PackageReference Include=""FluentAssertions"" Version=""5.10.3"" />
    <PackageReference Include=""Microsoft.AspNetCore.Mvc.Testing"" Version=""5.0.1"" />
    <PackageReference Include=""Microsoft.AspNet.WebApi.Client"" Version=""5.2.7"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""5.0.1"" />
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""16.8.3"" />
    <PackageReference Include=""Moq"" Version=""4.15.2"" />
    <PackageReference Include=""Respawn"" Version=""3.3.0"" />
    <PackageReference Include=""xunit"" Version=""2.4.1"" />
    <PackageReference Include=""xunit.runner.visualstudio"" Version=""2.4.3"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""coverlet.collector"" Version=""1.3.0"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\Application\Application.csproj"" />
    <ProjectReference Include=""..\Domain\Domain.csproj"" />
    <ProjectReference Include=""..\Infrastructure.Persistence\Infrastructure.Persistence.csproj"" />
    <ProjectReference Include=""..\Infrastructure.Shared\Infrastructure.Shared.csproj"" />
    <ProjectReference Include=""..\WebApi\WebApi.csproj"" />
  </ItemGroup>

</Project>";
        }
    }
}
