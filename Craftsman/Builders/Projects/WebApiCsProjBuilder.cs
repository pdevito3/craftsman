namespace Craftsman.Builders.Projects
{
    using System;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System.IO;
    using System.Text;
    using Enums;

    public class WebApiCsProjBuilder
    {
        public static void CreateWebApiCsProj(string solutionDirectory, string projectBaseName, string dbProvider)
        {
            var classPath = ClassPathHelper.WebApiProjectClassPath(solutionDirectory, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = "";
                data = GetWebApiCsProjFileText(solutionDirectory, projectBaseName, dbProvider);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static string GetWebApiCsProjFileText(string solutionDirectory, string projectBaseName, string dbProvider)
        {
            var sqlPackage = @$"<PackageReference Include=""Microsoft.EntityFrameworkCore.SqlServer"" Version=""5.0.6"" />";
            if (Enum.GetName(typeof(DbProvider), DbProvider.Postgres) == dbProvider)
                sqlPackage = @$"<PackageReference Include=""npgsql.entityframeworkcore.postgresql"" Version=""5.0.6"" />";
            //else if (Enum.GetName(typeof(DbProvider), DbProvider.MySql) == provider)
            //    return "UseMySql";
            
            return @$"<Project Sdk=""Microsoft.NET.Sdk.Web"">

  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
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
    <PackageReference Include=""AutoBogus"" Version=""2.13.0"" />
    <PackageReference Include=""Bogus"" Version=""33.0.2"" />
    <PackageReference Include=""Autofac.Extensions.DependencyInjection"" Version=""7.1.0"" />
    <PackageReference Include=""AutoMapper.Extensions.Microsoft.DependencyInjection"" Version=""8.1.1"" />
    <PackageReference Include=""FluentValidation.AspNetCore"" Version=""10.1.0"" />
    <PackageReference Include=""MediatR"" Version=""9.0.0"" />
    <PackageReference Include=""MediatR.Extensions.Microsoft.DependencyInjection"" Version=""9.0.0"" />
    <PackageReference Include=""Microsoft.AspNetCore.Authentication.OpenIdConnect"" Version=""5.0.6"" />
    <PackageReference Include=""Microsoft.AspNetCore.JsonPatch"" Version=""5.0.6"" />
    <PackageReference Include=""Microsoft.AspNetCore.Mvc.NewtonsoftJson"" Version=""5.0.6"" />
    <PackageReference Include=""Microsoft.AspNetCore.Mvc.Versioning"" Version=""5.0.0"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""5.0.6"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore.InMemory"" Version=""5.0.6"" />
    {sqlPackage}
    <PackageReference Include=""Microsoft.Extensions.Configuration.Binder"" Version=""5.0.0"" />
    <PackageReference Include=""Microsoft.AspNetCore.Authentication.JwtBearer"" Version=""5.0.6"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore.Design"" Version=""5.0.6"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""Swashbuckle.AspNetCore"" Version=""6.1.4"" />

    <PackageReference Include=""Sieve"" Version=""2.4.1"" />
    <PackageReference Include=""Serilog.AspNetCore"" Version=""4.1.0"" />
    <PackageReference Include=""Serilog.Enrichers.AspNetCore"" Version=""1.0.0"" />
    <PackageReference Include=""Serilog.Enrichers.Context"" Version=""4.2.0"" />
     <PackageReference Include=""Serilog.Exceptions"" Version=""7.1.0"" />
    <PackageReference Include=""Serilog.Enrichers.Process"" Version=""2.0.1"" />
    <PackageReference Include=""Serilog.Enrichers.Thread"" Version=""3.1.0"" />
    <PackageReference Include=""Serilog.Settings.Configuration"" Version=""3.1.0"" />
    <PackageReference Include=""Serilog.Sinks.Console"" Version=""3.1.1"" />
    <PackageReference Include=""Serilog.Sinks.Seq"" Version=""5.0.1"" />
  </ItemGroup>

</Project>";
        }
    }
}