namespace Craftsman.Helpers
{
    using Craftsman.Enums;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public static class ClassPathHelper
    {
        public const string SharedTestProjectSuffix = "SharedTestHelpers";
        public const string IntegrationTestProjectSuffix = "IntegrationTests";
        public const string FunctionalTestProjectSuffix = "FunctionalTests";
        public const string ApiProjectSuffix = "WebApi";
        public const string InfraProjectSuffix = "Inrastructure";
        public const string CoreProjectSuffix = "Core";

        public static ClassPath SolutionClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, "", className);
        }

        public static ClassPath ControllerClassPath(string solutionDirectory, string className, string projectBaseName, string version = "v1")
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{ApiProjectSuffix}", "Controllers", version), className);
        }

        public static ClassPath TestEntityIntegrationClassPath(string solutionDirectory, string className, string entityName, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{IntegrationTestProjectSuffix}", "IntegrationTests", entityName), className);
        }

        public static ClassPath WebApiExtensionsClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{ApiProjectSuffix}", "Extensions"), className);
        }

        public static ClassPath IntegrationTestUtilitiesClassPath(string projectDirectory, string projectBaseName, string className)
        {
            return new ClassPath(projectDirectory, Path.Combine($"{projectBaseName}.{IntegrationTestProjectSuffix}", "TestUtilities"), className);
        }

        public static ClassPath WebApiMiddlewareClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{ApiProjectSuffix}", "Middleware"), className);
        }

        public static ClassPath StartupClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, $"{projectBaseName}.{ApiProjectSuffix}", className);
        }

        public static ClassPath WebApiAppSettingsClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, $"{projectBaseName}.{ApiProjectSuffix}", className);
        }

        public static ClassPath WebApiLaunchSettingsClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{ApiProjectSuffix}", "Properties"), className);
        }

        public static ClassPath FeatureTestClassPath(string solutionDirectory, string className, string entityName, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{IntegrationTestProjectSuffix}","FeatureTests",entityName), className);
        }

        public static ClassPath TestFakesClassPath(string solutionDirectory, string className, string entityName, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{SharedTestProjectSuffix}","Fakes",entityName), className);
        }

        public static ClassPath EntityClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{CoreProjectSuffix}", "Entities"), className);
        }

        public static ClassPath SeederClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{InfraProjectSuffix}", "Seeders"), className);
        }

        public static ClassPath DbContextClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{InfraProjectSuffix}", "Contexts"), className);
        }

        public static ClassPath ValidationClassPath(string solutionDirectory, string className, string entityPlural, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{ApiProjectSuffix}", "Features", entityPlural, "Validators"), className);
        }

        public static ClassPath ProfileClassPath(string solutionDirectory, string className, string entityPlural, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{ApiProjectSuffix}", "Features", entityPlural, "Mappings"), className);
        }

        public static ClassPath FeaturesClassPath(string solutionDirectory, string className, string entityName, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{ApiProjectSuffix}", "Features", entityName), className);
        }

        public static ClassPath ApplicationInterfaceClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{CoreProjectSuffix}", "Interfaces"), className);
        }

        public static ClassPath CoreExceptionClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{CoreProjectSuffix}", "Exceptions"), className);
        }

        public static ClassPath WrappersClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{CoreProjectSuffix}", "Wrappers"), className);
        }

        public static ClassPath WebApiProjectRootClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{ApiProjectSuffix}"), className);
        }

        public static ClassPath ApplicationProjectRootClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{CoreProjectSuffix}"), className);
        }

        public static ClassPath IntegrationTestProjectRootClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, $"{projectBaseName}.{IntegrationTestProjectSuffix}", className);
        }

        public static ClassPath FunctionalTestProjectRootClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, $"{projectBaseName}.{FunctionalTestProjectSuffix}", className);
        }

        public static ClassPath SharedTestProjectRootClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, $"{projectBaseName}.{SharedTestProjectSuffix}", className);
        }

        public static ClassPath DtoClassPath(string solutionDirectory, string className, string entityName, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{CoreProjectSuffix}", "Dtos",entityName), className);
        }

        public static ClassPath SharedDtoClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{CoreProjectSuffix}", "Dtos", "Shared"), className);
        }

        public static ClassPath CoreProjectClassPath(string solutionDirectory, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{CoreProjectSuffix}"), $"{projectBaseName}.{CoreProjectSuffix}.csproj");
        }

        public static ClassPath InfrastructureProjectClassPath(string solutionDirectory, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{InfraProjectSuffix}"), $"{projectBaseName}.{InfraProjectSuffix}.csproj");
        }

        public static ClassPath InfrastructureServiceRegistrationClassPath(string solutionDirectory, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{InfraProjectSuffix}"), $"ServiceRegistration.cs");
        }

        public static ClassPath IntegrationTestProjectClassPath(string solutionDirectory, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, $"{projectBaseName}.{IntegrationTestProjectSuffix}", $"{projectBaseName}.{IntegrationTestProjectSuffix}.csproj");
        }

        public static ClassPath FunctionalTestProjectClassPath(string solutionDirectory, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, $"{projectBaseName}.{FunctionalTestProjectSuffix}", $"{projectBaseName}.{FunctionalTestProjectSuffix}.csproj");
        }

        public static ClassPath SharedTestProjectClassPath(string solutionDirectory, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, $"{projectBaseName}.{SharedTestProjectSuffix}", $"{projectBaseName}.{SharedTestProjectSuffix}.csproj");
        }

        public static ClassPath WebApiProjectClassPath(string solutionDirectory, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, $"{projectBaseName}.{ApiProjectSuffix}", $"{projectBaseName}.{ApiProjectSuffix}.csproj");
        }
    }
}
