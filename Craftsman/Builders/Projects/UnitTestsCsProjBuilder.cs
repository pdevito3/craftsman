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

    public class UnitTestsCsProjBuilder
    {
        public static void CreateTestsCsProj(string solutionDirectory, string projectBaseName)
        {
            var classPath = ClassPathHelper.UnitTestProjectClassPath(solutionDirectory, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = "";
                data = GetTestsCsProjFileText(solutionDirectory, projectBaseName);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static string GetTestsCsProjFileText(string solutionDirectory, string projectBaseName)
        {
            var coreClassPath = ClassPathHelper.CoreProjectClassPath(solutionDirectory, projectBaseName);
            var sharedTestClassPath = ClassPathHelper.SharedTestProjectClassPath(solutionDirectory, projectBaseName);

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
    <PackageReference Include=""Moq"" Version=""4.16.1"" />
    <PackageReference Include=""NUnit"" Version=""3.12.0"" />
    <PackageReference Include=""NUnit3TestAdapter"" Version=""3.16.1"" />
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""16.8.3"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\src\{coreClassPath.ClassNamespace}\{coreClassPath.ClassName}"" />
    <ProjectReference Include=""..\{sharedTestClassPath.ClassNamespace}\{sharedTestClassPath.ClassName}"" />
  </ItemGroup>

</Project>";
        }
    }
}