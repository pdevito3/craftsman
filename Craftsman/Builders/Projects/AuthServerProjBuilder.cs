namespace Craftsman.Builders.Projects
{
    using System;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;
    using Enums;

    public class AuthServerProjBuilder
    {
        public static void CreateProject(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiProjectClassPath(solutionDirectory, projectBaseName);
            var fileText = ProjectFileText();
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static string ProjectFileText()
        {
            return @$"<Project Sdk=""Microsoft.NET.Sdk.Web"">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Duende.IdentityServer"" Version=""5.2.1"" />    
    <PackageReference Include=""Microsoft.AspNetCore.Authentication.Google"" Version=""6.0.0-rc.2.*"" />
    <PackageReference Include=""Serilog.AspNetCore"" Version=""4.1.0"" />
  </ItemGroup>

  <Target Name=""Tailwind"" BeforeTargets=""Build"">
    <Exec Command=""npm install"" />
    <Exec Command=""npm run css:build"" />
  </Target>

</Project>";
        }
    }
}