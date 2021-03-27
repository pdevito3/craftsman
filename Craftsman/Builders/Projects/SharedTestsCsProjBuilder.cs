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

    public class SharedTestsCsProjBuilder
    {
        public static void CreateTestsCsProj(string solutionDirectory, string projectBaseName)
        {
            try
            {
                var classPath = ClassPathHelper.SharedTestProjectClassPath(solutionDirectory, projectBaseName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetInfrastructurePersistenceCsProjFileText(solutionDirectory, projectBaseName);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }
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

        public static string GetInfrastructurePersistenceCsProjFileText(string solutionDirectory, string projectBaseName)
        {
            var coreClassPath = ClassPathHelper.CoreProjectClassPath(solutionDirectory, projectBaseName);

            return @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""AutoBogus"" Version=""2.12.0"" />
    <PackageReference Include=""Bogus"" Version=""32.0.2"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\src\{coreClassPath.ClassNamespace}\{coreClassPath.ClassName}"" />
  </ItemGroup>

</Project>";
        }
    }
}
