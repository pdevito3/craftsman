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

        public static void AddProjects(string solutionDirectory, string projectDirectory, string dbProvider, string solutionName, bool addJwtAuth, IFileSystem fileSystem)
        {
            // add webapi first so it is default project
            BuildWebApiProject(solutionDirectory, projectDirectory, solutionName, addJwtAuth, fileSystem);
            BuildDomainProject(solutionDirectory, projectDirectory, "src");
            BuildApplicationProject(solutionDirectory, projectDirectory, "src", fileSystem);
            BuildInfrastructurePersistenceProject(solutionDirectory, projectDirectory, "src", dbProvider, fileSystem);
            BuildInfrastructureSharedProject(solutionDirectory, projectDirectory, "src", fileSystem);
            BuildTestProject(solutionDirectory, projectDirectory, "tests", solutionName, addJwtAuth);

            if (addJwtAuth)
                BuildInfrastructureIdentityProject(solutionDirectory, projectDirectory, "src");
        }

        private static void BuildDomainProject(string solutionDirectory, string projectDirectory, string solutionFolder)
        {
            var domainProjectClassPath = ClassPathHelper.DomainProjectClassPath(projectDirectory);

            DomainCsProjBuilder.CreateDomainCsProj(projectDirectory);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{domainProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

            // dir won't show up in project until file is added
            Directory.CreateDirectory(ClassPathHelper.EntityClassPath(projectDirectory, "").ClassDirectory);
        }

        private static void BuildApplicationProject(string solutionDirectory, string projectDirectory, string solutionFolder, IFileSystem fileSystem)
        {
            var applicationProjectClassPath = ClassPathHelper.ApplicationProjectClassPath(projectDirectory);

            ApplicationCsProjBuilder.CreateApplicationCsProj(projectDirectory);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{applicationProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

            // base folders
            Directory.CreateDirectory(ClassPathHelper.DtoClassPath(projectDirectory,"","").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.ApplicationExceptionClassPath(projectDirectory, "").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.ApplicationInterfaceClassPath(projectDirectory, "").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.ProfileClassPath(projectDirectory, "").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.ValidationClassPath(projectDirectory, "", "").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.WrappersClassPath(projectDirectory, "").ClassDirectory);

            Directory.CreateDirectory(ClassPathHelper.SharedDtoClassPath(projectDirectory, "").ClassDirectory);

            ApplicationServiceExtensionsBuilder.CreateApplicationServiceExtension(projectDirectory, fileSystem);
            BasePaginationParametersBuilder.CreateBasePaginationParameters(projectDirectory, fileSystem);
            PagedListBuilder.CreatePagedList(projectDirectory, fileSystem);
            ResponseBuilder.CreateResponse(projectDirectory, fileSystem);
            ApplicationExceptionsBuilder.CreateExceptions(projectDirectory);
        }

        private static void BuildInfrastructurePersistenceProject(string solutionDirectory, string projectDirectory, string solutionFolder, string dbProvider, IFileSystem fileSystem)
        {
            var infrastructurePersistenceProjectClassPath = ClassPathHelper.InfrastructurePersistenceProjectClassPath(projectDirectory);

            InfrastructurePersistenceCsProjBuilder.CreateInfrastructurePersistenceCsProj(projectDirectory, dbProvider);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{infrastructurePersistenceProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

            // base folders
            Directory.CreateDirectory(ClassPathHelper.DbContextClassPath(projectDirectory, "").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.RepositoryClassPath(projectDirectory, "").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.SeederClassPath(projectDirectory, "").ClassDirectory);

            InfrastructurePersistenceServiceRegistrationBuilder.CreateInfrastructurePersistenceServiceExtension(projectDirectory, fileSystem);
        }

        private static void BuildInfrastructureSharedProject(string solutionDirectory, string projectDirectory, string solutionFolder, IFileSystem fileSystem)
        {
            var infrastructureSharedProjectClassPath = ClassPathHelper.InfrastructureSharedProjectClassPath(projectDirectory);

            InfrastructureSharedCsProjBuilder.CreateInfrastructureSharedCsProj(projectDirectory);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{infrastructureSharedProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

            InfrastructureSharedServiceRegistrationBuilder.CreateInfrastructureSharedServiceExtension(projectDirectory, fileSystem);
        }

        private static void BuildInfrastructureIdentityProject(string solutionDirectory, string projectDirectory, string solutionFolder)
        {
            var infrastructureIdentityProjectClassPath = ClassPathHelper.InfrastructureIdentityProjectClassPath(projectDirectory);

            InfrastructureIdentityCsProjBuilder.CreateInfrastructureIdentityCsProj(projectDirectory);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{infrastructureIdentityProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
        }

        private static void BuildWebApiProject(string solutionDirectory, string projectDirectory, string solutionName, bool useJwtAuth, IFileSystem fileSystem)
        {
            var solutionFolder = projectDirectory.Replace(solutionDirectory, "").Replace(Path.DirectorySeparatorChar.ToString(), "");
            var webApiProjectClassPath = ClassPathHelper.WebApiProjectClassPath(projectDirectory, solutionName);

            WebApiCsProjBuilder.CreateWebApiCsProj(projectDirectory, solutionName, useJwtAuth);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{webApiProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

            // base folders
            Directory.CreateDirectory(ClassPathHelper.ControllerClassPath(projectDirectory, "", solutionName, "v1").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.WebApiExtensionsClassPath(projectDirectory, "", solutionName).ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.WebApiMiddlewareClassPath(projectDirectory, "", solutionName).ClassDirectory);

            WebApiServiceExtensionsBuilder.CreateWebApiServiceExtension(projectDirectory, solutionName, fileSystem);
            WebApiAppExtensionsBuilder.CreateWebApiAppExtension(projectDirectory, solutionName, fileSystem);
            ErrorHandlerMiddlewareBuilder.CreateErrorHandlerMiddleware(projectDirectory, solutionName, fileSystem);
            WebApiAppSettingsBuilder.CreateAppSettings(projectDirectory, solutionName);
            WebApiLaunchSettingsBuilder.CreateLaunchSettings(projectDirectory, solutionName, fileSystem);
            ProgramBuilder.CreateWebApiProgram(projectDirectory, solutionName, fileSystem);
            StartupBuilder.CreateWebApiStartup(projectDirectory, "Startup", useJwtAuth, solutionName);
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
