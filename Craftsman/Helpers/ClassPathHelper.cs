namespace Craftsman.Helpers
{
    using Craftsman.Models;
    using System.IO;

    public static class ClassPathHelper
    {
        public const string UnitTestProjectSuffix = "UnitTests";
        public const string SharedTestProjectSuffix = "SharedTestHelpers";
        public const string IntegrationTestProjectSuffix = "IntegrationTests";
        public const string FunctionalTestProjectSuffix = "FunctionalTests";
        // public const string ApiProjectSuffix = "WebApi";
        public const string ApiProjectSuffix = "";
        public const string MessagesProjName = "Messages";

        public static ClassPath SolutionClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, "", className);
        }

        public static ClassPath BaseMessageClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, MessagesProjName, className);
        }

        public static ClassPath ControllerClassPath(string solutionDirectory, string className, string projectBaseName, string version = "v1")
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}{withSuffix}", "Controllers", version), className);
        }

        public static ClassPath UnitTestClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{UnitTestProjectSuffix}", "UnitTests"), className);
        }

        public static ClassPath UnitTestWrapperTestsClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{UnitTestProjectSuffix}", "UnitTests", "Wrappers"), className);
        }

        public static ClassPath WebApiHostExtensionsClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine(projectBaseName, "Extensions", "Host"), className);
        }

        public static ClassPath WebApiServiceExtensionsClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}{withSuffix}", "Extensions", "Services"), className);
        }

        public static ClassPath WebApiConsumersServiceExtensionsClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}{withSuffix}", "Extensions", "Services", "ConsumerRegistrations"), className);
        }

        public static ClassPath WebApiProducersServiceExtensionsClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}{withSuffix}", "Extensions", "Services", "ProducerRegistrations"), className);
        }

        public static ClassPath WebApiApplicationExtensionsClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}{withSuffix}", "Extensions", "Application"), className);
        }

        public static ClassPath IntegrationTestUtilitiesClassPath(string projectDirectory, string projectBaseName, string className)
        {
            return new ClassPath(projectDirectory, Path.Combine($"{projectBaseName}.{IntegrationTestProjectSuffix}", "TestUtilities"), className);
        }

        public static ClassPath FunctionalTestUtilitiesClassPath(string projectDirectory, string projectBaseName, string className)
        {
            return new ClassPath(projectDirectory, Path.Combine($"{projectBaseName}.{FunctionalTestProjectSuffix}", "TestUtilities"), className);
        }

        public static ClassPath WebApiMiddlewareClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}{withSuffix}", "Middleware"), className);
        }

        public static ClassPath StartupClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(solutionDirectory, $"{projectBaseName}{withSuffix}", className);
        }

        public static ClassPath WebApiAppSettingsClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(solutionDirectory, $"{projectBaseName}{withSuffix}", className);
        }

        public static ClassPath AuthServerAppSettingsClassPath(string projectDirectory, string className, string authServerProjectName)
        {
            return new ClassPath(projectDirectory, authServerProjectName, className);
        }

        public static ClassPath WebApiLaunchSettingsClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}{withSuffix}", "Properties"), className);
        }

        public static ClassPath AuthServerLaunchSettingsClassPath(string projectDirectory, string className, string authServerProjectName)
        {
            return new ClassPath(projectDirectory, Path.Combine($"{authServerProjectName}", "Properties"), className);
        }

        public static ClassPath AuthServerConfigClassPath(string projectDirectory, string className, string authServerProjectName)
        {
            return new ClassPath(projectDirectory, Path.Combine($"{authServerProjectName}"), className);
        }

        public static ClassPath AuthServerPackageJsonClassPath(string projectDirectory, string className, string authServerProjectName)
        {
            return new ClassPath(projectDirectory, Path.Combine($"{authServerProjectName}"), className);
        }

        public static ClassPath AuthServerPostCssClassPath(string projectDirectory, string className, string authServerProjectName)
        {
            return new ClassPath(projectDirectory, Path.Combine($"{authServerProjectName}"), className);
        }

        public static ClassPath AuthServerControllersClassPath(string projectDirectory, string className, string authServerProjectName)
        {
            return new ClassPath(projectDirectory, Path.Combine($"{authServerProjectName}","Controllers"), className);
        }

        public static ClassPath AuthServerViewModelsClassPath(string projectDirectory, string className, string authServerProjectName)
        {
            return new ClassPath(projectDirectory, Path.Combine($"{authServerProjectName}","ViewModels"), className);
        }

        public static ClassPath AuthServerExtensionsClassPath(string projectDirectory, string className, string authServerProjectName)
        {
            return new ClassPath(projectDirectory, Path.Combine($"{authServerProjectName}","Extensions"), className);
        }

        public static ClassPath AuthServerModelsClassPath(string projectDirectory, string className, string authServerProjectName)
        {
            return new ClassPath(projectDirectory, Path.Combine($"{authServerProjectName}","Models"), className);
        }

        public static ClassPath AuthServerSeederClassPath(string projectDirectory, string className, string authServerProjectName)
        {
            return new ClassPath(projectDirectory, Path.Combine($"{authServerProjectName}","Seeders"), className);
        }

        public static ClassPath AuthServerAttributesClassPath(string projectDirectory, string className, string authServerProjectName)
        {
            return new ClassPath(projectDirectory, Path.Combine($"{authServerProjectName}","Attributes"), className);
        }

        public static ClassPath AuthServerCssClassPath(string projectDirectory, string className, string authServerProjectName)
        {
            return new ClassPath(projectDirectory, Path.Combine($"{authServerProjectName}","wwwroot", "css"), className);
        }

        public static ClassPath AuthServerViewsClassPath(string projectDirectory, string className, string authServerProjectName)
        {
            return new ClassPath(projectDirectory, Path.Combine($"{authServerProjectName}","Views"), className);
        }

        public enum AuthServerViewSubDir
        {
            Account,
            Shared
        }

        public static ClassPath AuthServerViewsSubDirClassPath(string projectDirectory, string className, string authServerProjectName, AuthServerViewSubDir subir)
        {
            var dirName = subir == AuthServerViewSubDir.Account ? "Account" : "Shared";
            
            return new ClassPath(projectDirectory, Path.Combine($"{authServerProjectName}","Views", dirName), className);
        }

        public static ClassPath AuthServerTailwindConfigClassPath(string projectDirectory, string className, string authServerProjectName)
        {
            return new ClassPath(projectDirectory, Path.Combine($"{authServerProjectName}"), className);
        }

        public static ClassPath FeatureTestClassPath(string solutionDirectory, string className, string entityName, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{IntegrationTestProjectSuffix}", "FeatureTests", entityName), className);
        }

        public static ClassPath FunctionalTestClassPath(string solutionDirectory, string className, string entityName, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{FunctionalTestProjectSuffix}", "FunctionalTests", entityName), className);
        }

        public static ClassPath TestFakesClassPath(string solutionDirectory, string className, string entityName, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}.{SharedTestProjectSuffix}", "Fakes", entityName), className);
        }

        public static ClassPath EntityClassPath(string solutionDirectory, string className, string entityPlural, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}{withSuffix}", "Domain", entityPlural), className);
        }

        public static ClassPath DummySeederClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}{withSuffix}", "Seeders", "DummyData"), className);
        }

        public static ClassPath DbContextClassPath(string srcDirectory, string className, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(srcDirectory, Path.Combine($"{projectBaseName}{withSuffix}", "Databases"), className);
        }

        public static ClassPath ValidationClassPath(string solutionDirectory, string className, string entityPlural, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}{withSuffix}", "Domain", entityPlural, "Validators"), className);
        }

        public static ClassPath ProfileClassPath(string solutionDirectory, string className, string entityPlural, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}{withSuffix}", "Domain", entityPlural, "Mappings"), className);
        }

        public static ClassPath FeaturesClassPath(string solutionDirectory, string className, string entityName, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return string.IsNullOrEmpty(entityName)
                ? new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}{withSuffix}", "Domain"), className)
                : new ClassPath(solutionDirectory,
                    Path.Combine($"{projectBaseName}{withSuffix}", "Domain", entityName, "Features"), className);
        }

        public static ClassPath ConsumerFeaturesClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}{withSuffix}", "Domain", "EventHandlers"), className);
        }

        public static ClassPath ProducerFeaturesClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}{withSuffix}", "Domain", "EventHandlers"), className);
        }

        public static ClassPath ExceptionsClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}{withSuffix}", "Exceptions"), className);
        }

        public static ClassPath WrappersClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}{withSuffix}", "Wrappers"), className);
        }

        public static ClassPath WebApiResourcesClassPath(string srcDirectory, string className, string projectBaseName)
        {
            return new ClassPath(srcDirectory, Path.Combine($"{projectBaseName}", "Resources"), className);
        }

        public static ClassPath WebApiServicesClassPath(string srcDirectory, string className, string projectBaseName)
        {
            return new ClassPath(srcDirectory, Path.Combine($"{projectBaseName}", "Services"), className);
        }

        public static ClassPath PolicyDomainClassPath(string srcDirectory, string className, string projectBaseName)
        {
            return new ClassPath(srcDirectory, Path.Combine($"{projectBaseName}", "Domain", "Policy"), className);
        }

        public static ClassPath WebApiProjectRootClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}{withSuffix}"), className);
        }

        public static ClassPath IntegrationTestProjectRootClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, $"{projectBaseName}.{IntegrationTestProjectSuffix}", className);
        }

        public static ClassPath UnitTestProjectRootClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, $"{projectBaseName}.{UnitTestProjectSuffix}", className);
        }

        public static ClassPath MessagesProjectRootClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, MessagesProjName, className);
        }
        
        public static ClassPath ExampleYamlRootClassPath(string solutionDirectory, string className)
        {
            return new ClassPath(solutionDirectory, "", className);
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
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}{withSuffix}", "Dtos", entityName), className);
        }

        public static ClassPath SharedDtoClassPath(string solutionDirectory, string className, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(solutionDirectory, Path.Combine($"{projectBaseName}{withSuffix}", "Dtos", "Shared"), className);
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

        public static ClassPath UnitTestProjectClassPath(string solutionDirectory, string projectBaseName)
        {
            return new ClassPath(solutionDirectory, $"{projectBaseName}.{UnitTestProjectSuffix}", $"{projectBaseName}.{UnitTestProjectSuffix}.csproj");
        }

        public static ClassPath MessagesProjectClassPath(string solutionDirectory)
        {
            return new ClassPath(solutionDirectory, MessagesProjName, $"{MessagesProjName}.csproj");
        }

        public static ClassPath WebApiProjectClassPath(string projectDirectory, string projectBaseName)
        {
            var withSuffix = ApiProjectSuffix.Length > 0 ? $".{ApiProjectSuffix}" : "";
            return new ClassPath(projectDirectory, $"{projectBaseName}{withSuffix}", $"{projectBaseName}{withSuffix}.csproj");
        }

        public static ClassPath AuthServerProjectClassPath(string projectDirectory, string authServerProjectName)
        {
            return new ClassPath(projectDirectory, $"{authServerProjectName}", $"{authServerProjectName}.csproj");
        }
    }
}