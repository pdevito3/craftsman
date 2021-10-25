namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System.IO.Abstractions;
    using System.Text;

    public class InfrastructureServiceRegistrationBuilder
    {
        public static void CreateInfrastructureServiceExtension(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"{Utilities.GetInfraRegistrationName()}.cs", projectBaseName);

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using var fs = fileSystem.File.Create(classPath.FullClassPath);
            var data = "";
            data = GetServiceRegistrationText(srcDirectory, projectBaseName, classPath.ClassNamespace);
            fs.Write(Encoding.UTF8.GetBytes(data));
        }

        public static string GetServiceRegistrationText(string srcDirectory, string projectBaseName, string classNamespace)
        {
            var dbContextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName);
            var utilsClassPath = ClassPathHelper.WebApiUtilsClassPath(srcDirectory, "", projectBaseName);
            return @$"namespace {classNamespace}
{{
    using {dbContextClassPath.ClassNamespace};
    using {utilsClassPath.ClassNamespace};
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceRegistration
    {{
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {{
            // DbContext -- Do Not Delete

            // Auth -- Do Not Delete
        }}
    }}
}}
";
        }
    }
}