namespace NewCraftsman.Builders.Projects
{
    using System.IO.Abstractions;
    using Helpers;
    using Services;

    public class AuthServerProjBuilder
    {
        private readonly ICraftsmanUtilities _utilities;

        public AuthServerProjBuilder(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }
        
        public void CreateProject(string solutionDirectory, string projectBaseName)
        {
            var classPath = ClassPathHelper.WebApiProjectClassPath(solutionDirectory, projectBaseName);
            var fileText = ProjectFileText();
            _utilities.CreateFile(classPath, fileText);
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
    <PackageReference Include=""Microsoft.AspNetCore.Authentication.Google"" Version=""6.0.0"" />
    <PackageReference Include=""Serilog.AspNetCore"" Version=""5.0.0"" />
  </ItemGroup>

  <Target Name=""Tailwind"" BeforeTargets=""Build"">
    <Exec Command=""npm install"" />
    <Exec Command=""npm run css:build"" />
  </Target>

</Project>";
        }
    }
}