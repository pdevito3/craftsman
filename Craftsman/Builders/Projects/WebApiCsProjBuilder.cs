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

    public class WebApiCsProjBuilder
    {
        public static void CreateWebApiCsProj(string solutionDirectory, string projectBaseName, bool addIdentity)
        {
            try
            {
                var classPath = ClassPathHelper.WebApiProjectClassPath(solutionDirectory, projectBaseName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetWebApiCsProjFileText(addIdentity);
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

        public static string GetWebApiCsProjFileText(bool addIdentity)
        {
            var identityProject = addIdentity ? $@"
    <ProjectReference Include=""..\Infrastructure.Identity\Infrastructure.Identity.csproj"" />" : "";

            return @$"<Project Sdk=""Microsoft.NET.Sdk.Web"">

  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>

  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|AnyCPU'"">
    <DocumentationFile></DocumentationFile>
    <NoWarn>1701;1702;</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Autofac.Extensions.DependencyInjection"" Version=""7.1.0"" />
    <PackageReference Include=""FluentValidation.AspNetCore"" Version=""9.3.0"" />
    <PackageReference Include=""Microsoft.AspNetCore.Authentication.OpenIdConnect"" Version=""5.0.1"" />
    <PackageReference Include=""Microsoft.AspNetCore.JsonPatch"" Version=""5.0.1"" />
    <PackageReference Include=""Microsoft.AspNetCore.Mvc.NewtonsoftJson"" Version=""5.0.1"" />
    <PackageReference Include=""Microsoft.AspNetCore.Mvc.Versioning"" Version=""4.2.0"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore.Design"" Version=""5.0.1"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""Swashbuckle.AspNetCore"" Version=""5.6.3"" />
    
    <PackageReference Include=""Serilog.AspNetCore"" Version=""3.4.0"" />
    <PackageReference Include=""Serilog.Enrichers.AspNetCore"" Version=""1.0.0"" />
    <PackageReference Include=""Serilog.Enrichers.Context"" Version=""4.2.0"" />
    <PackageReference Include=""Serilog.Enrichers.Environment"" Version=""2.1.3"" />
    <PackageReference Include=""Serilog.Enrichers.Process"" Version=""2.0.1"" />
    <PackageReference Include=""Serilog.Enrichers.Thread"" Version=""3.1.0"" />
    <PackageReference Include=""Serilog.Settings.Configuration"" Version=""3.1.0"" />
    <PackageReference Include=""Serilog.Sinks.Console"" Version=""3.1.1"" />
    <PackageReference Include=""Serilog.Sinks.Seq"" Version=""4.0.0"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\Application\Application.csproj"" />{identityProject}
    <ProjectReference Include=""..\Infrastructure.Persistence\Infrastructure.Persistence.csproj"" />
    <ProjectReference Include=""..\Infrastructure.Shared\Infrastructure.Shared.csproj"" />
  </ItemGroup>

</Project>";
        }
    }
}
