namespace Craftsman.Builders.Projects
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System.IO;
    using System.Text;

    public class CoreCsProjBuilder
    {
        public static void CreateCoreCsProj(string solutionDirectory, string solutionName)
        {
            var classPath = ClassPathHelper.CoreProjectClassPath(solutionDirectory, solutionName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = "";
                data = GetCoreCsProjFileText();
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static string GetCoreCsProjFileText()
        {
            return @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Sieve"" Version=""2.4.1"" />
    <PackageReference Include=""AutoMapper.Extensions.Microsoft.DependencyInjection"" Version=""8.1.1"" />
    <PackageReference Include=""FluentValidation"" Version=""10.1.0"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""5.0.6"" />
  </ItemGroup>

</Project>";
        }
    }
}