namespace Craftsman.Helpers
{
    using Craftsman.Enums;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;

    public static class ClassPathHelper
    {
        public static ClassPath ControllerClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, "WebApi\\Controllers\\v1", className);
        }

        public static ClassPath TestEntityIntegrationClassPath(string solutionDirectory, string className, string entityName, string solutionName)
        {
            return new ClassPath(solutionDirectory, $"{solutionName}.Tests\\IntegrationTests\\{entityName}", className);
        }

        public static ClassPath WebApiExtensionsClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, "WebApi\\Extensions", className);
        }

        public static ClassPath TestWebAppFactoryClassPath(string solutionDirectory, string className, string solutionName)
        {
            return new ClassPath(solutionDirectory, $"{solutionName}.Tests", className);
        }

        public static ClassPath StartupClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, "WebApi", className);
        }

        public static ClassPath AppSettingsClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, "WebApi", className);
        }

        public static ClassPath LaunchSettingsClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, "WebApi\\Properties", className);
        }

        public static ClassPath TestRepositoryClassPath(string solutionDirectory, string className, string entityName, string solutionName)
        {
            return new ClassPath(solutionDirectory, $"{solutionName}.Tests\\RepositoryTests\\{entityName}", className);
        }

        public static ClassPath TestFakesClassPath(string solutionDirectory, string className, string entityName, string solutionName)
        {
            return new ClassPath(solutionDirectory, $"{solutionName}.Tests\\Fakes\\{entityName}", className);
        }

        public static ClassPath InfraPersistenceServiceProviderClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, "Infrastructure.Persistence", className);
        }

        public static ClassPath EntityClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, "Domain\\Entities", className);
        }

        public static ClassPath SeederClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, "Infrastructure.Persistence\\Seeders", className);
        }

        public static ClassPath DbContextClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, "Infrastructure.Persistence\\Contexts", className);
        }

        public static ClassPath ValidationClassPath(string solutionDirectory, string className, string entityName)
        {
            return new ClassPath(solutionDirectory, $"Application\\Validation\\{entityName}", className);
        }

        public static ClassPath IRepositoryClassPath(string solutionDirectory, string className, string entityName)
        {
            return new ClassPath(solutionDirectory, $"Application\\Interfaces\\{entityName}", className);
        }

        public static ClassPath ProfileClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, $"Application\\Mappings", className);
        }

        public static ClassPath RepositoryClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, $"Infrastructure.Persistence\\Repositories", className);
        }

        public static ClassPath DtoClassPath(string solutionDirectory, string className, string entityName)
        {
            return new ClassPath(solutionDirectory, $"Application\\Dtos\\{entityName}", className);
        }
    }
}
