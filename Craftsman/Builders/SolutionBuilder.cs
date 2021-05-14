namespace Craftsman.Builders
{
    using Craftsman.Builders.Dtos;
    using Craftsman.Builders.Projects;
    using Craftsman.Builders.Tests.Utilities;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using static Helpers.ConsoleWriter;

    public class SolutionBuilder
    {
        public static void BuildSolution(string solutionDirectory, string solutionName, IFileSystem fileSystem)
        {
            try
            {
                fileSystem.Directory.CreateDirectory(solutionDirectory);
                Utilities.ExecuteProcess("dotnet", @$"new sln -n {solutionName}", solutionDirectory);
            }
            catch (Exception e)
            {
                WriteError(e.Message);
                throw;

                // custom error that you must be using the .net 5 sdk?
            }
        }

        public static void AddProjects(string solutionDirectory, string srcDirectory, string testDirectory, string dbProvider, string dbName, string projectBaseName, bool addJwtAuth, IFileSystem fileSystem)
        {
            // add webapi first so it is default project
            BuildWebApiProject(solutionDirectory, srcDirectory, projectBaseName, addJwtAuth, dbName, fileSystem);
            BuildCoreProject(solutionDirectory, srcDirectory, projectBaseName, fileSystem);
            BuildInfrastructureProject(solutionDirectory, srcDirectory, projectBaseName, dbProvider, fileSystem);
            BuildIntegrationTestProject(solutionDirectory, testDirectory, projectBaseName, addJwtAuth);
            BuildFunctionalTestProject(solutionDirectory, testDirectory, projectBaseName, addJwtAuth);
            BuildSharedTestProject(solutionDirectory, testDirectory, projectBaseName, addJwtAuth);
            BuildUnitTestProject(solutionDirectory, testDirectory, projectBaseName);
        }

        private static void BuildCoreProject(string solutionDirectory, string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var solutionFolder = srcDirectory.GetSolutionFolder(solutionDirectory);
            var coreProjectClassPath = ClassPathHelper.CoreProjectClassPath(srcDirectory, projectBaseName);

            CoreCsProjBuilder.CreateCoreCsProj(srcDirectory, projectBaseName);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{coreProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

            // dir won't show up in project until file is added
            Directory.CreateDirectory(ClassPathHelper.EntityClassPath(srcDirectory, "", projectBaseName).ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.DtoClassPath(srcDirectory, "", "", projectBaseName).ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.CoreExceptionClassPath(srcDirectory, "", projectBaseName).ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.WrappersClassPath(srcDirectory, "", projectBaseName).ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.SharedDtoClassPath(srcDirectory, "", projectBaseName).ClassDirectory);

            BasePaginationParametersBuilder.CreateBasePaginationParameters(srcDirectory, projectBaseName, fileSystem);
            PagedListBuilder.CreatePagedList(srcDirectory, projectBaseName, fileSystem);
            ResponseBuilder.CreateResponse(srcDirectory, projectBaseName, fileSystem);
            CoreExceptionsBuilder.CreateExceptions(srcDirectory, projectBaseName);
        }

        private static void BuildInfrastructureProject(string solutionDirectory, string testDirectory, string projectBaseName, string dbProvider, IFileSystem fileSystem)
        {
            var solutionFolder = testDirectory.GetSolutionFolder(solutionDirectory);
            var infrastructurePersistenceProjectClassPath = ClassPathHelper.InfrastructureProjectClassPath(testDirectory, projectBaseName);

            InfrastructureCsProjBuilder.CreateInfrastructurePersistenceCsProj(testDirectory, projectBaseName, dbProvider);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{infrastructurePersistenceProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

            // base folders
            Directory.CreateDirectory(ClassPathHelper.DbContextClassPath(testDirectory, "", projectBaseName).ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.SeederClassPath(testDirectory, "", projectBaseName).ClassDirectory);

            InfrastructureServiceRegistrationBuilder.CreateInfrastructureServiceExtension(testDirectory, projectBaseName, fileSystem);
        }

        private static void BuildWebApiProject(string solutionDirectory, string srcDirectory, string projectBaseName, bool useJwtAuth, string dbName, IFileSystem fileSystem)
        {
            var solutionFolder = srcDirectory.GetSolutionFolder(solutionDirectory);
            var webApiProjectClassPath = ClassPathHelper.WebApiProjectClassPath(srcDirectory, projectBaseName);

            WebApiCsProjBuilder.CreateWebApiCsProj(srcDirectory, projectBaseName);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{webApiProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

            // base folders
            Directory.CreateDirectory(ClassPathHelper.ControllerClassPath(srcDirectory, "", projectBaseName, "v1").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, "", projectBaseName).ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.WebApiMiddlewareClassPath(srcDirectory, "", projectBaseName).ClassDirectory);

            WebApiServiceExtensionsBuilder.CreateApiVersioningServiceExtension(srcDirectory, projectBaseName, fileSystem);
            WebApiServiceExtensionsBuilder.CreateCorsServiceExtension(srcDirectory, projectBaseName, fileSystem);
            WebApiServiceExtensionsBuilder.CreateWebApiServiceExtension(srcDirectory, projectBaseName, fileSystem);
            WebApiAppExtensionsBuilder.CreateErrorHandlerWebApiAppExtension(srcDirectory, projectBaseName, fileSystem);
            ErrorHandlerMiddlewareBuilder.CreateErrorHandlerMiddleware(srcDirectory, projectBaseName, fileSystem);
            WebApiAppSettingsBuilder.CreateAppSettings(srcDirectory, new ApiEnvironment(), dbName, projectBaseName); // empty environment so i can get a blank with all the right keys. don't care about values as they get overriden by env specific settings
            WebApiLaunchSettingsBuilder.CreateLaunchSettings(srcDirectory, projectBaseName, fileSystem);
            ProgramBuilder.CreateWebApiProgram(srcDirectory, projectBaseName, fileSystem);
            StartupBuilder.CreateWebApiStartup(srcDirectory, "Production", useJwtAuth, projectBaseName);
        }

        private static void BuildIntegrationTestProject(string solutionDirectory, string testDirectory, string projectBaseName, bool addJwtAuth)
        {
            var solutionFolder = testDirectory.GetSolutionFolder(solutionDirectory);
            var testProjectClassPath = ClassPathHelper.IntegrationTestProjectRootClassPath(testDirectory, "", projectBaseName);

            IntegrationTestsCsProjBuilder.CreateTestsCsProj(testDirectory, projectBaseName, addJwtAuth);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
        }

        private static void BuildFunctionalTestProject(string solutionDirectory, string testDirectory, string projectBaseName, bool addJwtAuth)
        {
            var solutionFolder = testDirectory.GetSolutionFolder(solutionDirectory);
            var testProjectClassPath = ClassPathHelper.FunctionalTestProjectRootClassPath(testDirectory, "", projectBaseName);

            FunctionalTestsCsProjBuilder.CreateTestsCsProj(testDirectory, projectBaseName, addJwtAuth);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
        }

        private static void BuildSharedTestProject(string solutionDirectory, string testDirectory, string projectBaseName, bool addJwtAuth)
        {
            var solutionFolder = testDirectory.GetSolutionFolder(solutionDirectory);
            var testProjectClassPath = ClassPathHelper.SharedTestProjectRootClassPath(testDirectory, "", projectBaseName);

            SharedTestsCsProjBuilder.CreateTestsCsProj(testDirectory, projectBaseName);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
        }

        private static void BuildUnitTestProject(string solutionDirectory, string testDirectory, string projectBaseName)
        {
            var solutionFolder = testDirectory.GetSolutionFolder(solutionDirectory);
            var testProjectClassPath = ClassPathHelper.UnitTestProjectRootClassPath(testDirectory, "", projectBaseName);

            UnitTestsCsProjBuilder.CreateTestsCsProj(testDirectory, projectBaseName);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
        }

        public static void BuildMessagesProject(string solutionDirectory, string messagesDirectory)
        {
            var messageProjectClassPath = ClassPathHelper.MessagesProjectRootClassPath(solutionDirectory, "");

            MessagesCsProjBuilder.CreateMessagesCsProj(solutionDirectory);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{messageProjectClassPath.FullClassPath}""", solutionDirectory);
        }
    }

    public static class Extensions
    {
        public static string GetSolutionFolder(this string projectDir, string solutionDir)
        {
            var folder = projectDir.Replace(solutionDir, "");

            return folder.Length > 0 ? folder.Substring(1) : folder;
        }
    }
}