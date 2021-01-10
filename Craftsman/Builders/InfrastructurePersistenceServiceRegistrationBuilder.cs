namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System;
    using System.IO.Abstractions;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class InfrastructurePersistenceServiceRegistrationBuilder
    {
        public static void CreateInfrastructurePersistenceServiceExtension(string solutionDirectory, IFileSystem fileSystem)
        {
            try
            {
                var classPath = ClassPathHelper.InfrastructurePersistenceProjectRootClassPath(solutionDirectory, $"ServiceRegistration.cs");

                if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                    fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

                if (fileSystem.File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (var fs = fileSystem.File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetServiceRegistrationText(classPath.ClassNamespace);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{fileSystem.Path.DirectorySeparatorChar}", ""));
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

        public static string GetServiceRegistrationText(string classNamespace)
        {
            return @$"namespace {classNamespace}
{{
    using Infrastructure.Persistence.Contexts;
    using Infrastructure.Persistence.Repositories;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Sieve.Services;

    public static class ServiceRegistration
    {{
        public static void AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {{
            #region DbContext -- Do Not Delete  
            #endregion

            services.AddScoped<SieveProcessor>();

            #region Repositories -- Do Not Delete
            #endregion
        }}
    }}
}}
";
        }
    }
}
