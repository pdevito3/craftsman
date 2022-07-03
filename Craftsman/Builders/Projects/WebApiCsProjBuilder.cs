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

    public void CreateWebApiCsProj(string solutionDirectory, string projectBaseName, DbProvider dbProvider)
    {
        var classPath = ClassPathHelper.WebApiProjectClassPath(solutionDirectory, projectBaseName);
        _utilities.CreateFile(classPath, GetWebApiCsProjFileText(dbProvider));
    }

    public static string GetWebApiCsProjFileText(DbProvider dbProvider)
    {
        return @$"<Project Sdk=""Microsoft.NET.Sdk.Web"">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|AnyCPU'"">
    <DocumentationFile></DocumentationFile>
    <NoWarn>1701;1702;</NoWarn>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Release|AnyCPU'"">
    <DocumentationFile></DocumentationFile>
    <NoWarn>1701;1702;</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Ardalis.SmartEnum"" Version=""2.1.0"" />
    <PackageReference Include=""AutoBogus"" Version=""2.13.1"" />
    <PackageReference Include=""Bogus"" Version=""34.0.2"" />
    <PackageReference Include=""AutoMapper.Extensions.Microsoft.DependencyInjection"" Version=""11.0.0"" />
    <PackageReference Include=""DateOnlyTimeOnly.AspNet"" Version=""1.0.3"" />
    <PackageReference Include=""DateOnlyTimeOnly.AspNet.Swashbuckle"" Version=""1.0.3"" />
    <PackageReference Include=""EFCore.NamingConventions"" Version=""6.0.0"" />
    <PackageReference Include=""FluentValidation.AspNetCore"" Version=""10.4.0"" />
    <PackageReference Include=""HeimGuard"" Version=""0.1.1"" />
    <PackageReference Include=""MediatR"" Version=""10.0.1"" />
    <PackageReference Include=""MediatR.Extensions.Microsoft.DependencyInjection"" Version=""10.0.1"" />
    <PackageReference Include=""Microsoft.AspNetCore.Authentication.OpenIdConnect"" Version=""6.0.4"" />
    <PackageReference Include=""Microsoft.AspNetCore.JsonPatch"" Version=""6.0.4"" />
    <PackageReference Include=""Microsoft.AspNetCore.Mvc.NewtonsoftJson"" Version=""6.0.4"" />
    <PackageReference Include=""Microsoft.AspNetCore.Mvc.Versioning"" Version=""5.0.0"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""6.0.4"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore.InMemory"" Version=""6.0.4"" />
    {dbProvider.PackageInclusionString("6.0.4")}
    <PackageReference Include=""Microsoft.Extensions.Configuration.Binder"" Version=""6.0.0"" />
    <PackageReference Include=""Microsoft.AspNetCore.Authentication.JwtBearer"" Version=""6.0.4"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore.Design"" Version=""6.0.4"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""Swashbuckle.AspNetCore"" Version=""6.3.0"" />

    <PackageReference Include=""Sieve"" Version=""2.5.5"" />
    <PackageReference Include=""Serilog.AspNetCore"" Version=""5.0.0"" />
    <PackageReference Include=""Serilog.Enrichers.AspNetCore"" Version=""1.0.0"" />
    <PackageReference Include=""Serilog.Enrichers.Context"" Version=""4.2.0"" />
    <PackageReference Include=""Serilog.Exceptions"" Version=""8.1.0"" />
    <PackageReference Include=""Serilog.Enrichers.Process"" Version=""2.0.2"" />
    <PackageReference Include=""Serilog.Enrichers.Thread"" Version=""3.1.0"" />
    <PackageReference Include=""Serilog.Settings.Configuration"" Version=""3.3.0"" />
    <PackageReference Include=""Serilog.Sinks.Console"" Version=""4.0.1"" />
    <PackageReference Include=""OpenTelemetry.Exporter.Jaeger"" Version=""1.2.0-rc5"" />
    <PackageReference Include=""OpenTelemetry.Extensions.Hosting"" Version=""1.0.0-rc9.2"" />
    <PackageReference Include=""OpenTelemetry.Instrumentation.AspNetCore"" Version=""1.0.0-rc9.2"" />
    <PackageReference Include=""OpenTelemetry.Instrumentation.SqlClient"" Version=""1.0.0-rc9.2"" />
    
  </ItemGroup>

</Project>";
    }
}
