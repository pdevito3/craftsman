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

    public class GatewayCsProjBuilder
    {
        public static void CreateGatewayCsProj(string solutionDirectory, string gatewayProjectName)
        {
            try
            {
                var classPath = ClassPathHelper.GatewayProjectClassPath(solutionDirectory, gatewayProjectName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetGatewayCsProjFileText();
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

        public static string GetGatewayCsProjFileText()
        {
            return @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>netcoreapp5.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Autofac.Extensions.DependencyInjection"" Version=""7.1.0"" />
    <PackageReference Include=""IdentityModel"" Version=""4.4.0"" />
    <PackageReference Include=""IdentityModel.AspNetCore"" Version=""2.0.0"" />
    <PackageReference Include=""Microsoft.AspNetCore.Authentication.JwtBearer"" Version=""5.0.1"" />
    <PackageReference Include=""Ocelot"" Version=""16.0.1"" />

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

</Project>";
        }
    }
}
