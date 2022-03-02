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
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class Utilities
    {
        public static string PropTypeCleanupDotNet(string prop)
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
        
        public static string PropTypeCleanupTypeScript(string prop)
        {
            return prop.ToLower() switch
            {
                "boolean" => "boolean",
                "bool" => "boolean",
                "number" => "number",
                "int" => "number",
                "string" => "string",
                "dateonly" => "Date",
                "timeonly" => "Date",
                "datetimeoffset" => "Date",
                "guid" => "string",
                "uuid" => "string",
                "boolean?" => "boolean?",
                "bool?" => "boolean?",
                "number?" => "number?",
                "int?" => "number?",
                "string?" => "string?",
                "dateonly?" => "Date?",
                "timeonly?" => "Date?",
                "datetimeoffset?" => "Date?",
                "guid?" => "string?",
                "uuid?" => "string?",
                _ => prop
            };
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

        public static string GetAppSettingsName(bool asJson = true)
        {
            return asJson ? $"appsettings.json" : $"appsettings";
        }

        public static string BffApiKeysFilename(string entityName)
        {
            return $"{entityName.LowercaseFirstLetter()}.keys";
        }

        public static string BffEntityListRouteComponentName(string entityName)
        {
            return $"{entityName.UppercaseFirstLetter()}List";
        }
        
        public static string BffApiKeysExport(string entityName)
        {
            return $"{entityName.UppercaseFirstLetter()}Keys";
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

        public static string FakeParentTestHelpers(Entity entity, out string fakeParentIdRuleFor)
        {
            var fakeParent = "";
            fakeParentIdRuleFor = "";
            foreach (var entityProperty in entity.Properties)
            {
                if (entityProperty.IsForeignKey && !entityProperty.IsMany && entityProperty.IsPrimativeType)
                {
                    var fakeParentClass = Utilities.FakerName(entityProperty.ForeignEntityName);
                    var fakeParentCreationDto =
                        Utilities.FakerName(Utilities.GetDtoName(entityProperty.ForeignEntityName, Dto.Creation));
                    fakeParent +=
                        @$"var fake{entityProperty.ForeignEntityName}One = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
        await InsertAsync(fake{entityProperty.ForeignEntityName}One);{Environment.NewLine}{Environment.NewLine}        ";
                    fakeParentIdRuleFor +=
                        $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{entityProperty.ForeignEntityName}One.Id){Environment.NewLine}            ";
                }
            }

            return fakeParent;
        }

        public static string FakeParentTestHelpersTwoCount(Entity entity, out string fakeParentIdRuleForOne, out string fakeParentIdRuleForTwo)
        {
            var fakeParent = "";
            fakeParentIdRuleForOne = "";
            fakeParentIdRuleForTwo = "";
            foreach (var entityProperty in entity.Properties)
            {
                if (entityProperty.IsForeignKey && !entityProperty.IsMany && entityProperty.IsPrimativeType)
                {
                    var fakeParentClass = Utilities.FakerName(entityProperty.ForeignEntityName);
                    var fakeParentCreationDto =
                        Utilities.FakerName(Utilities.GetDtoName(entityProperty.ForeignEntityName, Dto.Creation));
                    fakeParent +=
                        @$"var fake{entityProperty.ForeignEntityName}One = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
        var fake{entityProperty.ForeignEntityName}Two = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
        await InsertAsync(fake{entityProperty.ForeignEntityName}One, fake{entityProperty.ForeignEntityName}Two);{Environment.NewLine}{Environment.NewLine}        ";
                    fakeParentIdRuleForOne +=
                        $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{entityProperty.ForeignEntityName}One.Id){Environment.NewLine}            ";
                    fakeParentIdRuleForTwo +=
                        $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{entityProperty.ForeignEntityName}Two.Id){Environment.NewLine}            ";
                }
            }

            return fakeParent;
        }


        public static string FakeParentTestHelpersThreeCount(Entity entity, out string fakeParentIdRuleForOne, out string fakeParentIdRuleForTwo, out string fakeParentIdRuleForThree)
        {
            var fakeParent = "";
            fakeParentIdRuleForOne = "";
            fakeParentIdRuleForTwo = "";
            fakeParentIdRuleForThree = "";
            foreach (var entityProperty in entity.Properties)
            {
                if (entityProperty.IsForeignKey && !entityProperty.IsMany && entityProperty.IsPrimativeType)
                {
                    var fakeParentClass = Utilities.FakerName(entityProperty.ForeignEntityName);
                    var fakeParentCreationDto =
                        Utilities.FakerName(Utilities.GetDtoName(entityProperty.ForeignEntityName, Dto.Creation));
                    fakeParent +=
                        @$"var fake{entityProperty.ForeignEntityName}One = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
        var fake{entityProperty.ForeignEntityName}Two = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
        var fake{entityProperty.ForeignEntityName}Three = {fakeParentClass}.Generate(new {fakeParentCreationDto}().Generate());
        await InsertAsync(fake{entityProperty.ForeignEntityName}One, fake{entityProperty.ForeignEntityName}Two, fake{entityProperty.ForeignEntityName}Three);{Environment.NewLine}{Environment.NewLine}        ";
                    fakeParentIdRuleForOne +=
                        $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{entityProperty.ForeignEntityName}One.Id){Environment.NewLine}            ";
                    fakeParentIdRuleForTwo +=
                        $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{entityProperty.ForeignEntityName}Two.Id){Environment.NewLine}            ";
                    fakeParentIdRuleForThree +=
                        $"{Environment.NewLine}            .RuleFor({entity.Lambda} => {entity.Lambda}.{entityProperty.Name}, _ => fake{entityProperty.ForeignEntityName}Three.Id){Environment.NewLine}            ";
                }
            }

            return fakeParent;
        }

        public static string GetForeignEntityUsings(string testDirectory, Entity entity,
            string projectBaseName)
        {
            var foreignEntityUsings = "";
            var foreignProps = entity.Properties.Where(e => e.IsForeignKey).ToList();
            foreach (var entityProperty in foreignProps)
            {
                if (entityProperty.IsForeignKey && !entityProperty.IsMany)
                {
                    var parentClassPath =
                        ClassPathHelper.TestFakesClassPath(testDirectory, $"", entityProperty.ForeignEntityName, projectBaseName);

                    foreignEntityUsings += $@"
using {parentClassPath.ClassNamespace};";
                }
            }

            return foreignEntityUsings;
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

        public static string GetBffApiFilenameBase(string entityName, FeatureType type)
        {
            return type.Name switch
            {
                nameof(FeatureType.AddRecord) => $"add{entityName.UppercaseFirstLetter()}",
                nameof(FeatureType.GetList) => $"get{entityName.UppercaseFirstLetter()}List",
                nameof(FeatureType.GetRecord) => $"get{entityName.UppercaseFirstLetter()}",
                nameof(FeatureType.UpdateRecord) => $"update{entityName.UppercaseFirstLetter()}",
                nameof(FeatureType.DeleteRecord) => $"delete{entityName.UppercaseFirstLetter()}",
                _ => throw new Exception($"The '{type.Name}' feature is not supported in bff api scaffolding.")
            };
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

        public static void AddStartupEnvironmentsWithServices(
            string solutionDirectory,
            string dbName,
            ApiEnvironment environment,
            SwaggerConfig swaggerConfig,
            int port,
            string projectBaseName,
            DockerConfig dockerConfig,
            IFileSystem fileSystem)
        {
            AppSettingsBuilder.CreateWebApiAppSettings(solutionDirectory, dbName, projectBaseName);

            WebApiLaunchSettingsModifier.AddProfile(solutionDirectory, environment, port, dockerConfig, projectBaseName);
            if (!swaggerConfig.IsSameOrEqualTo(new SwaggerConfig()))
                SwaggerBuilder.RegisterSwaggerInStartup(solutionDirectory, projectBaseName);
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

        public static void GitSetup(string solutionDirectory, bool useSystemGitUser)
        {
            GitBuilder.CreateGitIgnore(solutionDirectory);

            Repository.Init(solutionDirectory);
            var repo = new Repository(solutionDirectory);

            string[] allFiles = Directory.GetFiles(solutionDirectory, "*.*", SearchOption.AllDirectories);
            Commands.Stage(repo, allFiles);

            var author = useSystemGitUser 
                ? repo.Config.BuildSignature(DateTimeOffset.Now) 
                : new Signature("Craftsman", "craftsman", DateTimeOffset.Now);
            repo.Commit("Initial Commit", author, author);
        }


        public static int GetFreePort()
        {
            // From https://stackoverflow.com/a/150974/4190785
            var tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();
            return port;
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
                return !string.IsNullOrEmpty(defaultValue) ? @$" = Guid.Parse(""{defaultValue}"");" : "";

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
        public const string GetRecord = $""{{Base}}/{lowercaseEntityPluralName}/{{{pkName}}}"";
        public const string Create = $""{{Base}}/{lowercaseEntityPluralName}"";
        public const string Delete = $""{{Base}}/{lowercaseEntityPluralName}/{{{pkName}}}"";
        public const string Put = $""{{Base}}/{lowercaseEntityPluralName}/{{{pkName}}}"";
        public const string Patch = $""{{Base}}/{lowercaseEntityPluralName}/{{{pkName}}}"";
        public const string CreateBatch = $""{{Base}}/{lowercaseEntityPluralName}/batch"";
    }}";

            return entityRouteClasses;
        }

        public static bool ProjectUsesSoftDelete(string srcDirectory, string projectBaseName)
        {
            var classPath = ClassPathHelper.EntityClassPath(srcDirectory, $"BaseEntity.cs", "", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                return false;

            if (!File.Exists(classPath.FullClassPath))
                return false;

            using var input = File.OpenText(classPath.FullClassPath);
            string line;
            while (null != (line = input.ReadLine()))
            {
                if (line.Contains($"Deleted"))
                    return true;
            }

            return false;
        }
    }
}
