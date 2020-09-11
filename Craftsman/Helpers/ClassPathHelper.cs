namespace Craftsman.Helpers
{
    using Craftsman.Enums;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public static class ClassPathHelper
    {        
        public static ClassPath IdentityProjectPath(string solutionDirectory)
        {
            return new ClassPath(solutionDirectory, "Infrastructure.Identity", "");
        }

        public static ClassPath ControllerClassPath(string solutionDirectory, string className, string version = "v1")
        {
            return new ClassPath(solutionDirectory, Path.Combine("WebApi","Controllers", version), className);
        }

        public static ClassPath TestEntityIntegrationClassPath(string solutionDirectory, string className, string entityName, string solutionName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{solutionName}.Tests", "IntegrationTests", entityName), className);
        }

        public static ClassPath WebApiExtensionsClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, Path.Combine("WebApi","Extensions"), className);
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
            return new ClassPath(solutionDirectory, Path.Combine("WebApi","Properties"), className);
        }

        public static ClassPath TestRepositoryClassPath(string solutionDirectory, string className, string entityName, string solutionName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{solutionName}.Tests","RepositoryTests",entityName), className);
        }

        public static ClassPath TestFakesClassPath(string solutionDirectory, string className, string entityName, string solutionName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{solutionName}.Tests","Fakes",entityName), className);
        }

        public static ClassPath InfraPersistenceServiceProviderClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, "Infrastructure.Persistence", className);
        }

        public static ClassPath EntityClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, Path.Combine("Domain","Entities"), className);
        }

        public static ClassPath SeederClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, Path.Combine("Infrastructure.Persistence","Seeders"), className);
        }

        public static ClassPath DbContextClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, Path.Combine("Infrastructure.Persistence","Contexts"), className);
        }

        public static ClassPath ValidationClassPath(string solutionDirectory, string className, string entityName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"Application","Validation",entityName), className);
        }

        public static ClassPath IRepositoryClassPath(string solutionDirectory, string className, string entityName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"Application","Interfaces",entityName), className);
        }

        public static ClassPath ProfileClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"Application","Mappings"), className);
        }

        public static ClassPath ApplicationInterfaceClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"Application", "Interfaces"), className);
        }

        public static ClassPath CommonDomainClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"Domain", "Common"), className);
        }

        public static ClassPath DomainEnumClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"Domain", "Enums"), className);
        }

        public static ClassPath DomainSettingsClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"Domain", "Settings"), className);
        }

        public static ClassPath RepositoryClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"Infrastructure.Persistence","Repositories"), className);
        }

        public static ClassPath DtoClassPath(string solutionDirectory, string className, string entityName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"Application","Dtos",entityName), className);
        }
    }
}
