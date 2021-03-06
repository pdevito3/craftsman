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

        public static void AddProjects(string solutionDirectory, string projectDirectory, string dbProvider, string soutionName, bool addJwtAuth, IFileSystem fileSystem)
        {
            // add webapi first so it is default project
            BuildWebApiProject(solutionDirectory, projectDirectory, "Api", addJwtAuth, fileSystem);
            BuildDomainProject(solutionDirectory, projectDirectory, "Core");
            BuildApplicationProject(solutionDirectory, projectDirectory, "Core", fileSystem);
            BuildInfrastructurePersistenceProject(solutionDirectory, projectDirectory, "Infrastructure", dbProvider, fileSystem);
            BuildInfrastructureSharedProject(solutionDirectory, projectDirectory, "Infrastructure", fileSystem);
            BuildTestProject(solutionDirectory, projectDirectory, "Tests", soutionName, addJwtAuth);

            if (addJwtAuth)
                BuildInfrastructureIdentityProject(solutionDirectory, projectDirectory, "Infrastructure");
        }

        public static void AddMicroServicesProjects(string solutionDirectory, string projectDirectory, string dbProvider, string projectBaseName, bool addJwtAuth, IFileSystem fileSystem)
        {
            var microSolutionFolder = Path.Combine("src", "services", projectBaseName);
            BuildWebApiProject(solutionDirectory, projectDirectory, microSolutionFolder, addJwtAuth, fileSystem, projectBaseName);
            BuildDomainProject(solutionDirectory, projectDirectory, microSolutionFolder);
            BuildApplicationProject(solutionDirectory, projectDirectory, microSolutionFolder, fileSystem);
            BuildInfrastructurePersistenceProject(solutionDirectory, projectDirectory, microSolutionFolder, dbProvider, fileSystem);
            BuildInfrastructureSharedProject(solutionDirectory, projectDirectory, microSolutionFolder, fileSystem);
            BuildTestProject(solutionDirectory, projectDirectory, microSolutionFolder, projectBaseName, addJwtAuth);

            if (addJwtAuth)
                BuildInfrastructureIdentityProject(solutionDirectory, projectDirectory, microSolutionFolder);
        }

        public static void AddGatewayProject(string solutionDirectory, string projectDirectory, string gatewayProjectName, IFileSystem fileSystem)
        {
            var microSolutionFolder = Path.Combine("src", "gateways");
            BuildGatewayProject(solutionDirectory, projectDirectory, microSolutionFolder, gatewayProjectName, fileSystem);
        }

        private static void BuildGatewayProject(string solutionDirectory, string projectDirectory, string solutionFolder, string gatewayProjectName, IFileSystem fileSystem)
        {
            var gatewayProjectClassPath = ClassPathHelper.GatewayProjectClassPath(projectDirectory, gatewayProjectName);

            GatewayCsProjBuilder.CreateGatewayCsProj(projectDirectory, gatewayProjectName);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{gatewayProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

            GatewayAppSettingsBuilder.CreateBaseAppSettings(projectDirectory, gatewayProjectName);
            GatewayLaunchSettingsBuilder.CreateLaunchSettings(projectDirectory, gatewayProjectName, fileSystem);
            ProgramBuilder.CreateGatewayProgram(projectDirectory, gatewayProjectName, fileSystem);
            StartupBuilder.CreateGatewayStartup(projectDirectory, "Startup", gatewayProjectName);
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

        private static void BuildWebApiProject(string solutionDirectory, string projectDirectory, string solutionFolder, bool useJwtAuth, IFileSystem fileSystem, string projectBaseName = "")
        {
            var webApiProjectClassPath = ClassPathHelper.WebApiProjectClassPath(projectDirectory, projectBaseName);

            WebApiCsProjBuilder.CreateWebApiCsProj(projectDirectory, projectBaseName, useJwtAuth);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{webApiProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

            // base folders
            Directory.CreateDirectory(ClassPathHelper.ControllerClassPath(projectDirectory, "", "v1", projectBaseName).ClassDirectory);
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
