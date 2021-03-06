namespace Craftsman.Helpers
{
    using Craftsman.Enums;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public static class ClassPathHelper
    {
        public static ClassPath SolutionClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, "", className);
        }

        public static ClassPath ControllerClassPath(string solutionDirectory, string className, string projectBaseName, string version = "v1")
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.WebApi", "Controllers", version), className);
        }

        public static ClassPath TestEntityIntegrationClassPath(string solutionDirectory, string className, string entityName, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.Tests", "IntegrationTests", entityName), className);
        }

        public static ClassPath WebApiExtensionsClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.WebApi", "Extensions"), className);
        }

        public static ClassPath HttpClientExtensionsClassPath(string projectDirectory, string projectBaseName, string className)
        {
            return new ClassPath(projectDirectory, Path.Combine($"{projectBaseName}.Tests", "Helpers"), className);
        }

        public static ClassPath WebApiMiddlewareClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.WebApi", "Middleware"), className);
        }

        public static ClassPath StartupClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, $"{projectBaseName}.WebApi", className);
        }

        public static ClassPath TestProjectClassPath(string solutionDirectory, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, $"{projectBaseName}.Tests", $"{projectBaseName}.Tests.csproj");
        }

        public static ClassPath WebApiProjectClassPath(string solutionDirectory, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, $"{projectBaseName}.WebApi", $"{projectBaseName}.WebApi.csproj");
        }

        public static ClassPath WebApiAppSettingsClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, $"{projectBaseName}.WebApi", className);
        }

        public static ClassPath WebApiLaunchSettingsClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.WebApi", "Properties"), className);
        }

        public static ClassPath TestRepositoryClassPath(string solutionDirectory, string className, string entityName, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.Tests","RepositoryTests",entityName), className);
        }

        public static ClassPath TestFakesClassPath(string solutionDirectory, string className, string entityName, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.Tests","Fakes",entityName), className);
        }

        public static ClassPath InfraPersistenceServiceProviderClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, "Infrastructure.Persistence", className);
        }

        public static ClassPath EntityClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.Core", "Entities"), className);
        }

        public static ClassPath SeederClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, Path.Combine("Infrastructure.Persistence","Seeders"), className);
        }

        public static ClassPath DbContextClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, Path.Combine("Infrastructure.Persistence","Contexts"), className);
        }

        public static ClassPath ValidationClassPath(string solutionDirectory, string className, string entityName, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.WebApi", "Features", entityName, "Validators"), className);
        }

        public static ClassPath ProfileClassPath(string solutionDirectory, string className, string entityName, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.WebApi", "Features", entityName, "Mappings"), className);
        }

        public static ClassPath ApplicationInterfaceClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.Core", "Interfaces"), className);
        }

        public static ClassPath CoreExceptionClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.Core", "Exceptions"), className);
        }

        public static ClassPath WrappersClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.Core", "Wrappers"), className);
        }

        public static ClassPath WebApiProjectRootClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.WebApi"), className);
        }

        public static ClassPath ApplicationProjectRootClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.Core"), className);
        }

        public static ClassPath InfrastructurePersistenceProjectRootClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"Infrastructure.Persistence"), className);
        }

        public static ClassPath TestProjectRootClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, $"{projectBaseName}.Tests", className);
        }

        public static ClassPath InfrastructureSharedProjectRootClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"Infrastructure.Shared"), className);
        }

        public static ClassPath InfrastructureIdentityProjectRootClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"Infrastructure.Identity"), className);
        }

        public static ClassPath RepositoryClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"Infrastructure.Persistence","Repositories"), className);
        }

        public static ClassPath DtoClassPath(string solutionDirectory, string className, string entityName, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.Core", "Dtos",entityName), className);
        }

        public static ClassPath SharedDtoClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.Core", "Dtos", "Shared"), className);
        }

        public static ClassPath CoreProjectClassPath(string solutionDirectory, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.Core"), $"{projectBaseName}.Core.csproj");
        }

        public static ClassPath InfrastructurePersistenceProjectClassPath(string solutionDirectory)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"Infrastructure.Persistence"), "Infrastructure.Persistence.csproj");
        }

        public static ClassPath InfrastructureIdentityProjectClassPath(string solutionDirectory)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"Infrastructure.Identity"), "Infrastructure.Identity.csproj");
        }
    }
}
