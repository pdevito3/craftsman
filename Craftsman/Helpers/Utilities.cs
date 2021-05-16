namespace Craftsman.Helpers
{
    using Craftsman.Builders;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Models;
    using FluentAssertions.Common;
    using LibGit2Sharp;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using static Helpers.ConsoleWriter;

    public class Utilities
    {
        public static string PropTypeCleanup(string prop)
        {
            var lowercaseProps = new string[] { "string", "int", "decimal", "double", "float", "object", "bool", "byte", "char", "byte", "ushort", "uint", "ulong" };
            if (lowercaseProps.Contains(prop.ToLower()))
                return prop.ToLower();
            else if (prop.ToLower() == "datetime")
                return "DateTime";
            else if (prop.ToLower() == "datetime?")
                return "DateTime?";
            else if (prop.ToLower() == "datetimeoffset")
                return "DateTimeOffset";
            else if (prop.ToLower() == "datetimeoffset?")
                return "DateTimeOffset?";
            else if (prop.ToLower() == "guid")
                return "Guid";
            else
                return prop;
        }

        public static ClassPath GetStartupClassPath(string envName, string solutionDirectory, string projectBaseName)
        {
            return envName == "Production"
                ? ClassPathHelper.StartupClassPath(solutionDirectory, $"Startup.cs", projectBaseName)
                : ClassPathHelper.StartupClassPath(solutionDirectory, $"Startup{envName}.cs", projectBaseName);
        }

        public static string SolutionGuard(string solutionDirectory)
        {
            var slnName = Directory.GetFiles(solutionDirectory, "*.sln").FirstOrDefault();
            return Path.GetFileNameWithoutExtension(slnName) ?? throw new SolutionNotFoundException();
        }

        public static void IsBoundedContextDirectoryGuard(string srcDirectory, string testDirectory)
        {
            if (!Directory.Exists(srcDirectory) || !Directory.Exists(testDirectory))
                throw new IsNotBoundedContextDirectory();
        }

        public static void IsSolutionDirectoryGuard(string proposedDirectory)
        {
            if (!Directory.EnumerateFiles(proposedDirectory, "*.sln").Any())
                throw new SolutionNotFoundException();
        }

        public static string CreateApiRouteClasses(List<Entity> entities)
        {
            var entityRouteClasses = "";

            foreach (var entity in entities)
            {
                var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();
                var pkName = entity.PrimaryKeyProperty.Name;

                entityRouteClasses += $@"{Environment.NewLine}{Environment.NewLine}public static class {entity.Plural}
        {{
            public const string {pkName} = ""{{{pkName.LowercaseFirstLetter()}}}"";
            public const string GetList = Base + ""/{lowercaseEntityPluralName}"";
            public const string GetRecord = Base + ""/{lowercaseEntityPluralName}/"" + {pkName};
            public const string Create = Base + ""/{lowercaseEntityPluralName}"";
            public const string Delete = Base + ""/{lowercaseEntityPluralName}/"" + {pkName};
            public const string Put = Base + ""/{lowercaseEntityPluralName}/"" + {pkName};
            public const string Patch = Base + ""/{lowercaseEntityPluralName}/"" + {pkName};
        }}";
            }

            return entityRouteClasses;
        }

        public static string GetRepositoryName(string entityName, bool isInterface)
        {
            return isInterface ? $"I{entityName}Repository" : $"{entityName}Repository";
        }

        public static string GetWebHostFactoryName()
        {
            return "TestingWebApplicationFactory";
        }

        public static string GetControllerName(string entityName)
        {
            return $"{entityName}Controller";
        }

        public static string GetSeederName(Entity entity)
        {
            return $"{entity.Name}Seeder";
        }

        public static string GetRepositoryListMethodName(string pluralEntity)
        {
            return $"Get{pluralEntity}Async";
        }

        public static string GetMassTransitRegistrationName()
        {
            return "MassTransitServiceExtension";
        }

        public static string GetAppSettingsName(string envName, bool asJson = true)
        {
            if (String.IsNullOrEmpty(envName))
                return asJson ? $"appsettings.json" : $"appsettings";

            return asJson ? $"appsettings.{envName}.json" : $"appsettings.{envName}";
        }

        public static string GetStartupName(string envName)
        {
            return envName == "Production" ? "Startup" : $"Startup{envName}";
        }

        public static string GetProfileName(string entityName)
        {
            return $"{entityName}Profile";
        }

        public static string GetBaseMessageName()
        {
            return $"IBaseMessage";
        }

        public static string GetIntegrationTestFixtureName()
        {
            return $"TestFixture";
        }

        public static string GetEntityFeatureClassName(string entityName)
        {
            return $"Get{entityName}";
        }

        public static string GetEntityListFeatureClassName(string entityName)
        {
            return $"Get{entityName}List";
        }

        public static string AddEntityFeatureClassName(string entityName)
        {
            return $"Add{entityName}";
        }

        public static string DeleteEntityFeatureClassName(string entityName)
        {
            return $"Delete{entityName}";
        }

        public static string UpdateEntityFeatureClassName(string entityName)
        {
            return $"Update{entityName}";
        }

        public static string PatchEntityFeatureClassName(string entityName)
        {
            return $"Patch{entityName}";
        }

        public static string QueryListName(string entityName)
        {
            return $"{entityName}ListQuery";
        }

        public static string QueryRecordName(string entityName)
        {
            return $"{entityName}Query";
        }

        public static string CommandAddName(string entityName)
        {
            return $"Add{entityName}Command";
        }

        public static string CommandDeleteName(string entityName)
        {
            return $"Delete{entityName}Command";
        }

        public static string CommandUpdateName(string entityName)
        {
            return $"Update{entityName}Command";
        }

        public static string CommandPatchName(string entityName)
        {
            return $"Patch{entityName}Command";
        }

        public static string FakerName(string objectToFakeName)
        {
            return $"Fake{objectToFakeName}";
        }

        public static string GetDtoName(string entityName, Dto dto)
        {
            switch (dto)
            {
                case Dto.Manipulation:
                    return $"{entityName}ForManipulationDto";

                case Dto.Creation:
                    return $"{entityName}ForCreationDto";

                case Dto.Update:
                    return $"{entityName}ForUpdateDto";

                case Dto.Read:
                    return $"{entityName}Dto";

                case Dto.ReadParamaters:
                    return $"{entityName}ParametersDto";

                default:
                    throw new Exception($"Name generator not configured for {Enum.GetName(typeof(Dto), dto)}");
            }
        }

        public static string ValidatorNameGenerator(string entityName, Validator validator)
        {
            switch (validator)
            {
                case Validator.Manipulation:
                    return $"{entityName}ForManipulationDtoValidator";

                case Validator.Creation:
                    return $"{entityName}ForCreationDtoValidator";

                case Validator.Update:
                    return $"{entityName}ForUpdateDtoValidator";

                default:
                    throw new Exception($"Name generator not configured for {Enum.GetName(typeof(Validator), validator)}");
            }
        }

        public static bool ExecuteProcess(string command, string args, string directory, Dictionary<string, string> envVariables, int killInterval = 15000, string processKilledMessage = "Process Killed.")
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = directory
                }
            };

            process.StartInfo.EnvironmentVariables[envVariables.Keys.FirstOrDefault()] = envVariables.Values.FirstOrDefault();

            process.Start();
            if (!process.WaitForExit(killInterval))
            {
                process.Kill();
                WriteWarning(processKilledMessage);
                return false;
            }
            return true;
        }

        public static void ExecuteProcess(string command, string args, string directory)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = directory
                }
            };

            process.Start();
            process.WaitForExit();
        }

        public static string EndpointBaseGenerator(string entityNamePlural)
        {
            return $@"api/{entityNamePlural}";
        }

        public static string PolicyStringBuilder(Policy policy)
        {
            if (policy.PolicyType == Enum.GetName(typeof(PolicyType), PolicyType.Scope))
            {
                // ex: options.AddPolicy("CanRead", policy => policy.RequireClaim("scope", "detailedrecipes.read"));
                return $@"                options.AddPolicy(""{policy.Name}"",
                    policy => policy.RequireClaim(""scope"", ""{policy.PolicyValue}""));";
            }
            //else if (policy.PolicyType == Enum.GetName(typeof(PolicyType), PolicyType.Role))
            //{
            //    // ex: options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Administrator"));
            //    return $@"                options.AddPolicy(""{policy.Name}"",
            //        policy => policy.RequireRole(""{policy.PolicyValue}""));";
            //}

            // claim ex: options.AddPolicy("EmployeeOnly", policy => policy.RequireClaim("EmployeeNumber"));
            return $@"                options.AddPolicy(""{policy.Name}"",
                    policy => policy.RequireClaim(""{policy.PolicyValue}""));";
        }

        public static string BuildTestAuthorizationString(List<Policy> policies, List<Endpoint> endpoints, string entityName, PolicyType policyType)
        {
            var endpointStrings = new List<string>();
            foreach (var endpoint in endpoints)
            {
                endpointStrings.Add(Enum.GetName(typeof(Endpoint), endpoint));
            }

            var results = policies
                .Where(p => p.EndpointEntities.Any(ee => ee.EntityName == entityName)
                    && p.EndpointEntities.Any(ee => ee.RestrictedEndpoints.Intersect(endpointStrings).Any()));

            return "{\"" + string.Join("\", \"", results.Select(r => r.PolicyValue)) + "\"}";
        }

        public static void AddStartupEnvironmentsWithServices(
            string solutionDirectory,
            string solutionName,
            string dbName,
            List<ApiEnvironment> environments,
            SwaggerConfig swaggerConfig,
            int port,
            bool useJwtAuth,
            string projectBaseName = "")
        {
            // add a development environment by default for local work if none exists
            if (environments.Where(e => e.EnvironmentName == "Development").Count() == 0)
                environments.Add(new ApiEnvironment { EnvironmentName = "Development", ProfileName = $"{solutionName} (Development)" });

            if (environments.Where(e => e.EnvironmentName == "Production").Count() == 0)
                environments.Add(new ApiEnvironment { EnvironmentName = "Production", ProfileName = $"{solutionName} (Production)" });

            var sortedEnvironments = environments.OrderBy(e => e.EnvironmentName == "Development" ? 1 : 0).ToList(); // sets dev as default profile
            foreach (var env in sortedEnvironments)
            {
                // default startup is already built in cleanup phase
                if (env.EnvironmentName != "Production")
                    StartupBuilder.CreateWebApiStartup(solutionDirectory, env.EnvironmentName, useJwtAuth, projectBaseName);

                WebApiAppSettingsBuilder.CreateAppSettings(solutionDirectory, env, dbName, projectBaseName);
                WebApiLaunchSettingsModifier.AddProfile(solutionDirectory, env, port, projectBaseName);

                //services
                if (!swaggerConfig.IsSameOrEqualTo(new SwaggerConfig()))
                    SwaggerBuilder.RegisterSwaggerInStartup(solutionDirectory, env, projectBaseName);
            }

            // add an integration testing env to make sure that an in memory database is used
            var functionalEnv = new ApiEnvironment() { EnvironmentName = "FunctionalTesting" };
            WebApiAppSettingsBuilder.CreateAppSettings(solutionDirectory, functionalEnv, "", projectBaseName);
        }

        public static List<Policy> GetEndpointPolicies(List<Policy> policies, Endpoint endpoint, string entityName)
        {
            return policies
                .Where(p => p.EndpointEntities.Any(ee => ee.EntityName == entityName)
                    && p.EndpointEntities.Any(ee => ee.RestrictedEndpoints.Any(re => re == Enum.GetName(typeof(Endpoint), endpoint))))
                .ToList();
        }

        public static string GetForeignKeyIncludes(Entity entity)
        {
            var fkIncludes = "";
            foreach (var fk in entity.Properties.Where(p => p.IsForeignKey))
            {
                fkIncludes += $@"{Environment.NewLine}                .Include({fk.Name.ToLower().Substring(0, 1)} => {fk.Name.ToLower().Substring(0, 1)}.{fk.Name})";
            }

            return fkIncludes;
        }

        public static void GitSetup(string solutionDirectory)
        {
            GitBuilder.CreateGitIgnore(solutionDirectory);

            Repository.Init(solutionDirectory);
            var repo = new Repository(solutionDirectory);

            string[] allFiles = Directory.GetFiles(solutionDirectory, "*.*", SearchOption.AllDirectories);
            Commands.Stage(repo, allFiles);

            var author = new Signature("Craftsman", "craftsman", DateTimeOffset.Now);
            repo.Commit("Initial Commit", author, author);
        }

        public static void AddPackages(ClassPath classPath, Dictionary<string, string> packagesToAdd)
        {
            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using var output = new StreamWriter(tempPath);
                string line;
                var packagesAdded = false;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"PackageReference") && !packagesAdded)
                    {
                        newText += @$"{ProjectReferencePackagesString(packagesToAdd)}";
                        packagesAdded = true;
                    }

                    output.WriteLine(newText);
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);
        }

        private static string ProjectReferencePackagesString(Dictionary<string, string> packagesToAdd)
        {
            var packageString = "";
            foreach (var package in packagesToAdd)
            {
                packageString += $@"{Environment.NewLine}    <PackageReference Include=""{package.Key}"" Version=""{package.Value}"" />";
            }

            return packageString;
        }

        public static void AddProjectReference(ClassPath classPath, string relativeProjectPath)
        {
            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using var output = new StreamWriter(tempPath);
                string line;
                var projectAdded = false;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"ProjectReference") && !projectAdded)
                    {
                        newText += @$"
    <ProjectReference Include=""{relativeProjectPath}"" />";
                        projectAdded = true;
                    }

                    output.WriteLine(newText);
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);
        }

        public static string GetDefaultValueText(string defaultValue, string propType)
        {
            if (propType == "string")
                return defaultValue == null ? "" : @$" = ""{defaultValue}"";";

            return defaultValue == null ? "" : $" = {defaultValue};";
        }
    }
}