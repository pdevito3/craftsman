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

    public class InfrastructureCsProjBuilder
    {
        public static void CreateInfrastructurePersistenceCsProj(string solutionDirectory, string projectBaseName, string dbProvider)
        {
            try
            {
                var classPath = ClassPathHelper.InfrastructureProjectClassPath(solutionDirectory, projectBaseName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetInfrastructurePersistenceCsProjFileText(solutionDirectory, projectBaseName, dbProvider);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }
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

        public static string GetInfrastructurePersistenceCsProjFileText(string solutionDirectory, string projectBaseName, string dbProvider)
        {
            var coreClassPath = ClassPathHelper.CoreProjectClassPath(solutionDirectory, projectBaseName);
            var sqlPackage = @$"<PackageReference Include=""Microsoft.EntityFrameworkCore.SqlServer"" Version=""5.0.0"" />";
            if (Enum.GetName(typeof(DbProvider), DbProvider.Postgres) == dbProvider)
                sqlPackage = @$"<PackageReference Include=""npgsql.entityframeworkcore.postgresql"" Version=""5.0.0"" />";
            //else if (Enum.GetName(typeof(DbProvider), DbProvider.MySql) == provider)
            //    return "UseMySql";

            return @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""AutoBogus"" Version=""2.12.0"" />
    <PackageReference Include=""Bogus"" Version=""32.0.2"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""5.0.1"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore.InMemory"" Version=""5.0.1"" />
    {sqlPackage}
    <PackageReference Include=""Microsoft.Extensions.Configuration.Binder"" Version=""5.0.0"" />
    <PackageReference Include=""Microsoft.AspNetCore.Authentication.JwtBearer"" Version=""5.0.1"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\{coreClassPath.ClassNamespace}\{coreClassPath.ClassName}"" />
  </ItemGroup>

</Project>";
        }
    }
}
