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

    public class ApplicationCsProjBuilder
    {
        public static void CreateApplicationCsProj(string solutionDirectory)
        {
            try
            {
                var classPath = ClassPathHelper.ApplicationProjectClassPath(solutionDirectory);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetApplicationCsProjFileText();
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

        public static string GetApplicationCsProjFileText()
        {
            return @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""AutoMapper.Extensions.Microsoft.DependencyInjection"" Version=""8.1.0"" />
    <PackageReference Include=""FluentValidation"" Version=""9.3.0"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""5.0.1"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\Domain\Domain.csproj"" />
  </ItemGroup>

</Project>";
        }
    }
}
