namespace Craftsman.Builders.Projects
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System.IO;
    using System.Text;

    public class SharedTestsCsProjBuilder
    {
        public static void CreateTestsCsProj(string solutionDirectory, string projectBaseName)
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

        public static string GetInfrastructurePersistenceCsProjFileText(string solutionDirectory, string projectBaseName)
        {
            var apiClassPath = ClassPathHelper.WebApiProjectClassPath(solutionDirectory, projectBaseName);

            return @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""AutoBogus"" Version=""2.13.0"" />
    <PackageReference Include=""Bogus"" Version=""33.0.2"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\src\{apiClassPath.ClassNamespace}\{apiClassPath.ClassName}"" />
  </ItemGroup>

</Project>";
        }
    }
}