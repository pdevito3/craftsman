namespace Craftsman.Builders
{
    using Craftsman.Builders.Dtos;
    using Craftsman.Builders.Projects;
    using Craftsman.Builders.Tests.IntegrationTests;
    using Craftsman.Helpers;
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

        public static void AddProjects(string solutionDirectory, string srcDirectory, string testDirectory, string dbProvider, string projectBaseName, bool addJwtAuth, IFileSystem fileSystem)
        {
            // add webapi first so it is default project
            BuildWebApiProject(solutionDirectory, srcDirectory, projectBaseName, addJwtAuth, fileSystem);
            BuildCoreProject(solutionDirectory, srcDirectory, projectBaseName, fileSystem);
            BuildInfrastructurePersistenceProject(solutionDirectory, srcDirectory, projectBaseName, dbProvider, fileSystem);
            BuildTestProject(solutionDirectory, srcDirectory, "tests", projectBaseName, addJwtAuth);

            if (addJwtAuth)
                BuildInfrastructureIdentityProject(solutionDirectory, srcDirectory, "src");
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

            CoreServiceExtensionsBuilder.CreateCoreServiceExtension(projectDirectory, projectBaseName, fileSystem);
            BasePaginationParametersBuilder.CreateBasePaginationParameters(projectDirectory, projectBaseName, fileSystem);
            PagedListBuilder.CreatePagedList(projectDirectory, projectBaseName, fileSystem);
            ResponseBuilder.CreateResponse(projectDirectory, projectBaseName, fileSystem);
            CoreExceptionsBuilder.CreateExceptions(projectDirectory, projectBaseName);
        }

        private static void BuildInfrastructurePersistenceProject(string solutionDirectory, string projectDirectory, string projectBaseName, string dbProvider, IFileSystem fileSystem)
        {
            var solutionFolder = projectDirectory.Replace(solutionDirectory, "").Replace(Path.DirectorySeparatorChar.ToString(), "");
            var infrastructurePersistenceProjectClassPath = ClassPathHelper.InfrastructurePersistenceProjectClassPath(projectDirectory);

            InfrastructurePersistenceCsProjBuilder.CreateInfrastructurePersistenceCsProj(projectDirectory, projectBaseName, dbProvider);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{infrastructurePersistenceProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

            // base folders
            Directory.CreateDirectory(ClassPathHelper.DbContextClassPath(projectDirectory, "").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.RepositoryClassPath(projectDirectory, "").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.SeederClassPath(projectDirectory, "").ClassDirectory);

            InfrastructurePersistenceServiceRegistrationBuilder.CreateInfrastructurePersistenceServiceExtension(projectDirectory, projectBaseName, fileSystem);
        }

        private static void BuildInfrastructureIdentityProject(string solutionDirectory, string projectDirectory, string solutionFolder)
        {
            var infrastructureIdentityProjectClassPath = ClassPathHelper.InfrastructureIdentityProjectClassPath(projectDirectory);

            InfrastructureIdentityCsProjBuilder.CreateInfrastructureIdentityCsProj(projectDirectory);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{infrastructureIdentityProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
        }

        private static void BuildWebApiProject(string solutionDirectory, string projectDirectory, string projectBaseName, bool useJwtAuth, IFileSystem fileSystem)
        {
            var solutionFolder = projectDirectory.Replace(solutionDirectory, "").Replace(Path.DirectorySeparatorChar.ToString(), "");
            var webApiProjectClassPath = ClassPathHelper.WebApiProjectClassPath(projectDirectory, projectBaseName);

            WebApiCsProjBuilder.CreateWebApiCsProj(projectDirectory, projectBaseName, useJwtAuth);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{webApiProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

            // base folders
            Directory.CreateDirectory(ClassPathHelper.ControllerClassPath(projectDirectory, "", projectBaseName, "v1").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.WebApiExtensionsClassPath(projectDirectory, "", projectBaseName).ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.WebApiMiddlewareClassPath(projectDirectory, "", projectBaseName).ClassDirectory);

            WebApiServiceExtensionsBuilder.CreateWebApiServiceExtension(projectDirectory, projectBaseName, fileSystem);
            WebApiAppExtensionsBuilder.CreateWebApiAppExtension(projectDirectory, projectBaseName, fileSystem);
            ErrorHandlerMiddlewareBuilder.CreateErrorHandlerMiddleware(projectDirectory, projectBaseName, fileSystem);
            WebApiAppSettingsBuilder.CreateAppSettings(projectDirectory, projectBaseName);
            WebApiLaunchSettingsBuilder.CreateLaunchSettings(projectDirectory, projectBaseName, fileSystem);
            ProgramBuilder.CreateWebApiProgram(projectDirectory, projectBaseName, fileSystem);
            StartupBuilder.CreateWebApiStartup(projectDirectory, "Startup", useJwtAuth, projectBaseName);
        }

        private static void BuildTestProject(string solutionDirectory, string projectDirectory, string solutionFolder, string projectBaseName, bool addJwtAuth)
        {
            var testProjectClassPath = ClassPathHelper.TestProjectRootClassPath(projectDirectory, "", projectBaseName);

            TestsCsProjBuilder.CreateTestsCsProj(projectDirectory, projectBaseName, addJwtAuth);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

            HealthCheckTestBuilder.CreateHealthCheckTests(projectDirectory, projectBaseName);

            if (addJwtAuth)
            {
                Directory.CreateDirectory(ClassPathHelper.HttpClientExtensionsClassPath(projectDirectory, projectBaseName, "").ClassDirectory);
                HttpClientExtensionsBuilder.Create(projectDirectory, projectBaseName);
            }
        }
    }
}
