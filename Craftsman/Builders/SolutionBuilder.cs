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
            catch(Exception e)
            {
                WriteError(e.Message);
                throw;

                // custom error that you must be using the .net 5 sdk?
            }
        }

        public static void AddProjects(string solutionDirectory, string srcDirectory, string testDirectory, string dbProvider, string dbContextName, string projectBaseName, bool addJwtAuth, IFileSystem fileSystem)
        {
            // add webapi first so it is default project
            BuildWebApiProject(solutionDirectory, srcDirectory, projectBaseName, addJwtAuth, dbContextName, fileSystem);
            BuildCoreProject(solutionDirectory, srcDirectory, projectBaseName, fileSystem);
            BuildInfrastructureProject(solutionDirectory, srcDirectory, projectBaseName, dbProvider, fileSystem);
            BuildIntegrationTestProject(solutionDirectory, testDirectory, projectBaseName, addJwtAuth);
            BuildFunctionalTestProject(solutionDirectory, testDirectory, projectBaseName, addJwtAuth);
            BuildSharedTestProject(solutionDirectory, testDirectory, projectBaseName, addJwtAuth);
            BuildUnitTestProject(solutionDirectory, testDirectory, projectBaseName);
        }

        private static void BuildCoreProject(string solutionDirectory, string projectDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var solutionFolder = projectDirectory.Replace(solutionDirectory, "").Replace(Path.DirectorySeparatorChar.ToString(), "");
            var coreProjectClassPath = ClassPathHelper.CoreProjectClassPath(projectDirectory, projectBaseName);

            CoreCsProjBuilder.CreateCoreCsProj(projectDirectory, projectBaseName);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{coreProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

            // dir won't show up in project until file is added
            Directory.CreateDirectory(ClassPathHelper.EntityClassPath(projectDirectory, "", projectBaseName).ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.DtoClassPath(projectDirectory, "", "", projectBaseName).ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.CoreExceptionClassPath(projectDirectory, "", projectBaseName).ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.WrappersClassPath(projectDirectory, "", projectBaseName).ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.SharedDtoClassPath(projectDirectory, "", projectBaseName).ClassDirectory);

            BasePaginationParametersBuilder.CreateBasePaginationParameters(projectDirectory, projectBaseName, fileSystem);
            PagedListBuilder.CreatePagedList(projectDirectory, projectBaseName, fileSystem);
            ResponseBuilder.CreateResponse(projectDirectory, projectBaseName, fileSystem);
            CoreExceptionsBuilder.CreateExceptions(projectDirectory, projectBaseName);
        }

        private static void BuildInfrastructureProject(string solutionDirectory, string projectDirectory, string projectBaseName, string dbProvider, IFileSystem fileSystem)
        {
            var solutionFolder = projectDirectory.Replace(solutionDirectory, "").Replace(Path.DirectorySeparatorChar.ToString(), "");
            var infrastructurePersistenceProjectClassPath = ClassPathHelper.InfrastructureProjectClassPath(projectDirectory, projectBaseName);

            InfrastructureCsProjBuilder.CreateInfrastructurePersistenceCsProj(projectDirectory, projectBaseName, dbProvider);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{infrastructurePersistenceProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

            // base folders
            Directory.CreateDirectory(ClassPathHelper.DbContextClassPath(projectDirectory, "", projectBaseName).ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.SeederClassPath(projectDirectory, "", projectBaseName).ClassDirectory);

            InfrastructureServiceRegistrationBuilder.CreateInfrastructureServiceExtension(projectDirectory, projectBaseName, fileSystem);
        }

        private static void BuildWebApiProject(string solutionDirectory, string projectDirectory, string projectBaseName, bool useJwtAuth, string dbContextName, IFileSystem fileSystem)
        {
            var solutionFolder = projectDirectory.Replace(solutionDirectory, "").Replace(Path.DirectorySeparatorChar.ToString(), "");
            var webApiProjectClassPath = ClassPathHelper.WebApiProjectClassPath(projectDirectory, projectBaseName);

            WebApiCsProjBuilder.CreateWebApiCsProj(projectDirectory, projectBaseName);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{webApiProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

            // base folders
            Directory.CreateDirectory(ClassPathHelper.ControllerClassPath(projectDirectory, "", projectBaseName, "v1").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.WebApiExtensionsClassPath(projectDirectory, "", projectBaseName).ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.WebApiMiddlewareClassPath(projectDirectory, "", projectBaseName).ClassDirectory);

            WebApiServiceExtensionsBuilder.CreateWebApiServiceExtension(projectDirectory, projectBaseName, fileSystem);
            WebApiAppExtensionsBuilder.CreateWebApiAppExtension(projectDirectory, projectBaseName, fileSystem);
            ErrorHandlerMiddlewareBuilder.CreateErrorHandlerMiddleware(projectDirectory, projectBaseName, fileSystem);
            WebApiAppSettingsBuilder.CreateAppSettings(projectDirectory, new ApiEnvironment(), dbContextName, projectBaseName); // empty environment so i can get a blank with all the right keys. don't care about values as they get overriden by env specific settings
            WebApiLaunchSettingsBuilder.CreateLaunchSettings(projectDirectory, projectBaseName, fileSystem);
            ProgramBuilder.CreateWebApiProgram(projectDirectory, projectBaseName, fileSystem);
            StartupBuilder.CreateWebApiStartup(projectDirectory, "Production", useJwtAuth, projectBaseName);
        }

        private static void BuildIntegrationTestProject(string solutionDirectory, string testDirectory, string projectBaseName, bool addJwtAuth)
        {
            var solutionFolder = testDirectory.Replace(solutionDirectory, "").Replace(Path.DirectorySeparatorChar.ToString(), "");
            var testProjectClassPath = ClassPathHelper.IntegrationTestProjectRootClassPath(testDirectory, "", projectBaseName);

            IntegrationTestsCsProjBuilder.CreateTestsCsProj(testDirectory, projectBaseName, addJwtAuth);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
        }

        private static void BuildFunctionalTestProject(string solutionDirectory, string testDirectory, string projectBaseName, bool addJwtAuth)
        {
            var solutionFolder = testDirectory.Replace(solutionDirectory, "").Replace(Path.DirectorySeparatorChar.ToString(), "");
            var testProjectClassPath = ClassPathHelper.FunctionalTestProjectRootClassPath(testDirectory, "", projectBaseName);

            FunctionalTestsCsProjBuilder.CreateTestsCsProj(testDirectory, projectBaseName, addJwtAuth);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
        }

        private static void BuildSharedTestProject(string solutionDirectory, string testDirectory, string projectBaseName, bool addJwtAuth)
        {
            var solutionFolder = testDirectory.Replace(solutionDirectory, "").Replace(Path.DirectorySeparatorChar.ToString(), "");
            var testProjectClassPath = ClassPathHelper.SharedTestProjectRootClassPath(testDirectory, "", projectBaseName);

            SharedTestsCsProjBuilder.CreateTestsCsProj(testDirectory, projectBaseName);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
        }

        private static void BuildUnitTestProject(string solutionDirectory, string testDirectory, string projectBaseName)
        {
            var solutionFolder = testDirectory.Replace(solutionDirectory, "").Replace(Path.DirectorySeparatorChar.ToString(), "");
            var testProjectClassPath = ClassPathHelper.UnitTestProjectRootClassPath(testDirectory, "", projectBaseName);

            UnitTestsCsProjBuilder.CreateTestsCsProj(testDirectory, projectBaseName);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
        }
    }
}
