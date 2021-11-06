namespace Craftsman.Helpers
{
    using Craftsman.Builders;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Models;
    using FluentAssertions.Common;
    using LibGit2Sharp;
    using Spectre.Console;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Tracing;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class Utilities
    {
        public static string PropTypeCleanup(string prop)
        {
            var lowercaseProps = new string[] { "string", "int", "decimal", "double", "float", "object", "bool", "char", "byte", "ushort", "uint", "ulong" };
            if (lowercaseProps.Contains(prop.ToLower()))
                return prop.ToLower();
            else if (prop.ToLower() == "datetime")
                return "DateTime";
            else if (prop.ToLower() == "datetime?")
                return "DateTime?";
            else if (prop.ToLower() == "dateonly?")
                return "DateOnly?";
            else if (prop.ToLower() == "dateonly")
                return "DateOnly";
            else if (prop.ToLower() == "timeonly?")
                return "TimeOnly?";
            else if (prop.ToLower() == "timeonly")
                return "TimeOnly";
            else if (prop.ToLower() == "datetimeoffset")
                return "DateTimeOffset";
            else if (prop.ToLower() == "datetimeoffset?")
                return "DateTimeOffset?";
            else if (prop.ToLower() == "guid")
                return "Guid";
            else
                return prop;
        }

        public static ClassPath GetStartupClassPath(string solutionDirectory, string projectBaseName)
        {
            return ClassPathHelper.StartupClassPath(solutionDirectory, $"Startup.cs", projectBaseName);
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

        public static string GetRepositoryName(string entityName, bool isInterface)
        {
            return isInterface ? $"I{entityName}Repository" : $"{entityName}Repository";
        }

        public static string GetApiRouteClass(string entityPlural)
        {
            return entityPlural;
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

        public static string GetInfraRegistrationName()
        {
            return "InfrastructureServiceExtension";
        }

        public static string GetSwaggerServiceExtensionName()
        {
            return "SwaggerServiceExtension";
        }

        public static List<Policy> GetPoliciesThatDoNotExist(List<Policy> policies, string existingFileFullClassPath)
        {
            var nonExistantPolicies = new List<Policy>();
            nonExistantPolicies.AddRange(policies);

            var fileText = File.ReadAllText(existingFileFullClassPath);

            foreach (var policy in policies)
            {
                if (fileText.Contains(policy.Name) || fileText.Contains(policy.PolicyValue))
                {
                    nonExistantPolicies.Remove(policy);
                }
            }

            return nonExistantPolicies;
        }

        public static string GetAppSettingsName(string envName, bool asJson = true)
        {
            if (String.IsNullOrEmpty(envName))
                return asJson ? $"appsettings.json" : $"appsettings";

            return asJson ? $"appsettings.{envName}.json" : $"appsettings.{envName}";
        }

        public static string GetProfileName(string entityName)
        {
            return $"{entityName}Profile";
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
            return $@"api/{entityNamePlural.ToLower()}";
        }

        public static string PolicyInfraStringBuilder(Policy policy)
        {
            if (policy.PolicyType == Enum.GetName(typeof(PolicyType), PolicyType.Scope))
            {
                // ex: options.AddPolicy("CanRead", policy => policy.RequireClaim("scope", "detailedrecipes.read"));
                return $@"            options.AddPolicy(""{policy.Name}"",
                    policy => policy.RequireClaim(""scope"", ""{policy.PolicyValue}""));";
            }
            //else if (policy.PolicyType == Enum.GetName(typeof(PolicyType), PolicyType.Role))
            //{
            //    // ex: options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Administrator"));
            //    return $@"                options.AddPolicy(""{policy.Name}"",
            //        policy => policy.RequireRole(""{policy.PolicyValue}""));";
            //}

            // claim ex: options.AddPolicy("EmployeeOnly", policy => policy.RequireClaim("EmployeeNumber"));
            return $@"            options.AddPolicy(""{policy.Name}"",
                    policy => policy.RequireClaim(""{policy.PolicyValue}""));";
        }

        public static object GetSwaggerPolicies(IEnumerable<Policy> policies)
        {
            var policyStrings = "";

            //var uniquePolicies = policies.DistinctBy(); //TODO use with .net6
            var uniquePolicies = policies
                .GroupBy(p => new {p.Name, p.PolicyValue, p.PolicyType} )
                .Select(g => g.First())
                .ToList();
            foreach (var policy in uniquePolicies)
            {
                policyStrings += $@"
                            {{ ""{policy.PolicyValue}"",""{policy.Name}"" }},";
            }

            return policyStrings;
        }

        public static string BuildTestAuthorizationString(List<Policy> policies, List<Endpoint> endpoints, string entityName, PolicyType policyType)
        {
            return "{\"" + string.Join("\", \"", policies.Select(r => r.PolicyValue)) + "\"}";
        }

        public static void AddStartupEnvironmentsWithServices(
            string solutionDirectory,
            string projectName,
            string dbName,
            List<ApiEnvironment> environments,
            SwaggerConfig swaggerConfig,
            int port,
            bool useJwtAuth,
            string projectBaseName,
            IFileSystem fileSystem)
        {
            foreach (var env in environments)
            {
                AppSettingsBuilder.CreateWebApiAppSettings(solutionDirectory, env, dbName, projectBaseName);
                WebApiLaunchSettingsModifier.AddProfile(solutionDirectory, env, port, projectBaseName);

                //services
            }
            if (!swaggerConfig.IsSameOrEqualTo(new SwaggerConfig()))
                SwaggerBuilder.RegisterSwaggerInStartup(solutionDirectory, projectBaseName);
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

        public static void CreateFile(ClassPath classPath, string fileText, IFileSystem fileSystem)
        {
            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using var fs = fileSystem.File.Create(classPath.FullClassPath);
            fs.Write(Encoding.UTF8.GetBytes(fileText));
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
                    if (line.Contains($"</Project>") && !projectAdded)
                    {
                        newText = @$"
  <ItemGroup>
    <ProjectReference Include=""{relativeProjectPath}"" />
  </ItemGroup>

{newText}";
                        projectAdded = true;
                    }

                    output.WriteLine(newText);
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);
        }

        public static string GetDefaultValueText(string defaultValue, EntityProperty prop)
        {
            if (prop.Type == "string")
                return defaultValue == null ? "" : @$" = ""{defaultValue}"";";

            if ((prop.Type.IsGuidPropertyType() && !prop.Type.Contains("?") && !prop.IsForeignKey))
                return !string.IsNullOrEmpty(defaultValue) ? @$" = Guid.Parse(""{defaultValue}"");" : @" = Guid.NewGuid();";
            
            return string.IsNullOrEmpty(defaultValue) ? "" : $" = {defaultValue};";
        }

        public static string GetDbContext(string srcDirectory, string projectBaseName)
        {
            var classPath = ClassPathHelper.DbContextClassPath(srcDirectory, $"", projectBaseName);
            var directoryClasses = Directory.GetFiles(classPath.FullClassPath, "*.cs");
            foreach (var directoryClass in directoryClasses)
            {
                using var input = File.OpenText(directoryClass);
                string line;
                while (null != (line = input.ReadLine()))
                {
                    if (line.Contains($": DbContext"))
                        return Path.GetFileNameWithoutExtension(directoryClass);
                }
            }

            return "";
        }

        public static string GetRandomId(string idType)
        {
            if (idType.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                return @$"""badKey""";

            if (idType.Equals("guid", StringComparison.InvariantCultureIgnoreCase))
                return @$"Guid.NewGuid()";

            return idType.Equals("int", StringComparison.InvariantCultureIgnoreCase) ? @$"84709321" : "";
        }

        private static bool RunDbMigration(ApiTemplate template, string srcDirectory)
        {
            var webApiProjectClassPath = ClassPathHelper.WebApiProjectClassPath(srcDirectory, template.ProjectName);

            return ExecuteProcess(
                "dotnet",
                @$"ef migrations add ""InitialMigration"" --project ""{webApiProjectClassPath.FullClassPath}""",
                srcDirectory,
                new Dictionary<string, string>()
                {
                    { "ASPNETCORE_ENVIRONMENT", Guid.NewGuid().ToString() } // guid to not conflict with any given envs
                },
                20000,
                $"{Emoji.Known.Warning} {template.ProjectName} Database Migrations timed out and will need to be run manually");
        }

        public static void RunDbMigrations(List<ApiTemplate> boundedContexts, string domainDirectory)
        {
            AnsiConsole.Status()
                .AutoRefresh(true)
                .Spinner(Spinner.Known.Dots2)
                .Start($"[yellow]Running Migrations [/]", ctx =>
                {
                    foreach (var bc in boundedContexts)
                    {
                        var bcDirectory = $"{domainDirectory}{Path.DirectorySeparatorChar}{bc.ProjectName}";
                        var srcDirectory = Path.Combine(bcDirectory, "src");

                        ctx.Spinner(Spinner.Known.Dots2);
                        ctx.Status($"[bold blue]Running {bc.ProjectName} Database Migrations [/]");
                        if (Utilities.RunDbMigration(bc, srcDirectory))
                            WriteLogMessage($"Database Migrations for {bc.ProjectName} were successful");
                    }
                });
        }
        
        public static string CreateApiRouteClasses(Entity entity)
        {
            var entityRouteClasses = "";

            var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();
            var pkName = Entity.PrimaryKeyProperty.Name;

            entityRouteClasses += $@"{Environment.NewLine}{Environment.NewLine}public static class {entity.Plural}
    {{
        public const string {pkName} = ""{{{pkName.LowercaseFirstLetter()}}}"";
        public const string GetList = $""{{Base}}/{lowercaseEntityPluralName}"";
        public const string GetRecord = $""{{Base}}/{lowercaseEntityPluralName}/ + {pkName}"";
        public const string Create = $""{{Base}}/{lowercaseEntityPluralName}"";
        public const string Delete = $""{{Base}}/{lowercaseEntityPluralName}/ + {pkName}"";
        public const string Put = $""{{Base}}/{lowercaseEntityPluralName}/ + {pkName}"";
        public const string Patch = $""{{Base}}/{lowercaseEntityPluralName}/ + {pkName}"";
    }}";

            return entityRouteClasses;
        }
    }
}