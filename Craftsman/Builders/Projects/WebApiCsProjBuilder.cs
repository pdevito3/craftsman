namespace Craftsman.Builders.Projects;

using Domain;
using Helpers;
using Services;

public class WebApiCsProjBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public WebApiCsProjBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateWebApiCsProj(string solutionDirectory, string projectBaseName, DbProvider dbProvider, bool useCustomErrorHandler)
    {
        var classPath = ClassPathHelper.WebApiProjectClassPath(solutionDirectory, projectBaseName);
        _utilities.CreateFile(classPath, GetWebApiCsProjFileText(dbProvider, useCustomErrorHandler));
    }

    public static string GetWebApiCsProjFileText(DbProvider dbProvider, bool useCustomErrorHandler)
    {
        var errorPackages = "";
        if (!useCustomErrorHandler)
            errorPackages = $@"{Environment.NewLine}    <PackageReference Include=""Hellang.Middleware.ProblemDetails"" Version=""6.5.1"" />";
        
        return @$"<Project Sdk=""Microsoft.NET.Sdk.Web"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>

  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|AnyCPU'"">
    <DocumentationFile></DocumentationFile>
    <NoWarn>1701;1702;8632;</NoWarn>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Release|AnyCPU'"">
    <DocumentationFile></DocumentationFile>
    <NoWarn>1701;1702;8632;</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Asp.Versioning.Mvc"" Version=""8.0.0"" />
    <PackageReference Include=""Asp.Versioning.Mvc.ApiExplorer"" Version=""8.0.0"" />
    <PackageReference Include=""Ardalis.SmartEnum"" Version=""7.0.0"" />
    <PackageReference Include=""Destructurama.Attributed"" Version=""3.1.0"" />
    <PackageReference Include=""EFCore.NamingConventions"" Version=""8.0.1"" />
    <PackageReference Include=""FluentValidation.AspNetCore"" Version=""11.3.0"" />
    <PackageReference Include=""Hangfire"" Version=""1.8.6"" />
    <PackageReference Include=""Hangfire.MemoryStorage"" Version=""1.8.0"" />
    <PackageReference Include=""HeimGuard"" Version=""1.0.0"" />{errorPackages}
    <PackageReference Include=""MediatR"" Version=""12.2.0"" />
    <PackageReference Include=""Microsoft.AspNetCore.Authentication.OpenIdConnect"" Version=""8.0.0"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""8.0.0"" />
    {dbProvider.ApiPackageInclusionString("8.0.0")}
    <PackageReference Include=""Microsoft.Extensions.Configuration.Binder"" Version=""8.0.0"" />
    <PackageReference Include=""Microsoft.AspNetCore.Authentication.JwtBearer"" Version=""8.0.0"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore.Design"" Version=""8.0.0"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""OpenTelemetry.Exporter.Jaeger"" Version=""1.5.1"" />
    <PackageReference Include=""OpenTelemetry.Extensions.Hosting"" Version=""1.7.0"" />
    <PackageReference Include=""OpenTelemetry.Instrumentation.AspNetCore"" Version=""1.7.0"" />
    <PackageReference Include=""OpenTelemetry.Instrumentation.SqlClient"" Version=""1.5.1-beta.1"" />
    <PackageReference Include=""OpenTelemetry.Instrumentation.EntityFrameworkCore"" Version=""1.0.0-beta.7"" />
    <PackageReference Include=""OpenTelemetry.Instrumentation.EventCounters"" Version=""1.5.1-alpha.1"" />
    <PackageReference Include=""OpenTelemetry.Instrumentation.Http"" Version=""1.7.0"" />
    <PackageReference Include=""OpenTelemetry.Instrumentation.Runtime"" Version=""1.5.1"" />
    <PackageReference Include=""QueryKit"" Version=""1.1.0"" />
    <PackageReference Include=""Riok.Mapperly"" Version=""3.3.0"" />
    <PackageReference Include=""Swashbuckle.AspNetCore"" Version=""6.5.0"" />

    <PackageReference Include=""Serilog.AspNetCore"" Version=""8.0.0"" />
    <PackageReference Include=""Serilog.Enrichers.AspNetCore"" Version=""1.0.0"" />
    <PackageReference Include=""Serilog.Enrichers.Environment"" Version=""2.3.0"" />
    <PackageReference Include=""Serilog.Exceptions"" Version=""8.4.0"" />
    <PackageReference Include=""Serilog.Enrichers.Process"" Version=""2.0.2"" />
    <PackageReference Include=""Serilog.Enrichers.Thread"" Version=""3.1.0"" />
    <PackageReference Include=""Serilog.Settings.Configuration"" Version=""8.0.0"" />
    <PackageReference Include=""Serilog.Sinks.Console"" Version=""5.0.1"" />    
  </ItemGroup>

</Project>";
    }
}
