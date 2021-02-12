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

        public static void AddMicroServicesProjects(string solutionDirectory, string projectDirectory, string dbProvider, string solutionName, bool addJwtAuth, IFileSystem fileSystem)
        {
            var microSolutionFolder = Path.Combine("src", "services", solutionName);
            BuildWebApiProject(solutionDirectory, projectDirectory, microSolutionFolder, addJwtAuth, fileSystem);
            BuildDomainProject(solutionDirectory, projectDirectory, microSolutionFolder);
            BuildApplicationProject(solutionDirectory, projectDirectory, microSolutionFolder, fileSystem);
            BuildInfrastructurePersistenceProject(solutionDirectory, projectDirectory, microSolutionFolder, dbProvider, fileSystem);
            BuildInfrastructureSharedProject(solutionDirectory, projectDirectory, microSolutionFolder, fileSystem);
            BuildTestProject(solutionDirectory, projectDirectory, microSolutionFolder, solutionName, addJwtAuth);

            if (addJwtAuth)
                BuildInfrastructureIdentityProject(solutionDirectory, projectDirectory, "Infrastructure");
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

        private static void BuildWebApiProject(string solutionDirectory, string projectDirectory, string solutionFolder, bool useJwtAuth, IFileSystem fileSystem)
        {
            var webApiProjectClassPath = ClassPathHelper.WebApiProjectClassPath(projectDirectory);

            WebApiCsProjBuilder.CreateWebApiCsProj(projectDirectory);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{webApiProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

            // base folders
            Directory.CreateDirectory(ClassPathHelper.ControllerClassPath(projectDirectory, "").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.WebApiExtensionsClassPath(projectDirectory, "").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.WebApiMiddlewareClassPath(projectDirectory, "").ClassDirectory);

            WebApiServiceExtensionsBuilder.CreateWebApiServiceExtension(projectDirectory, fileSystem);
            WebApiAppExtensionsBuilder.CreateWebApiAppExtension(projectDirectory, fileSystem);
            ErrorHandlerMiddlewareBuilder.CreateErrorHandlerMiddleware(projectDirectory, fileSystem);
            WebApiAppSettingsBuilder.CreateAppSettings(projectDirectory);
            WebApiLaunchSettingsBuilder.CreateLaunchSettings(projectDirectory, fileSystem);
            ProgramBuilder.CreateWebApiProgram(projectDirectory, fileSystem);
            StartupBuilder.CreateWebApiStartup(projectDirectory, "Startup", useJwtAuth);
        }

        private static void BuildTestProject(string solutionDirectory, string projectDirectory, string solutionFolder, string solutionName, bool addJwtAuth)
        {
            var testProjectClassPath = ClassPathHelper.TestProjectRootClassPath(projectDirectory, "", solutionName);

            TestsCsProjBuilder.CreateTestsCsProj(projectDirectory, solutionName, addJwtAuth);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

            HealthCheckTestBuilder.CreateHealthCheckTests(projectDirectory, solutionName);

            if (addJwtAuth)
            {
                Directory.CreateDirectory(ClassPathHelper.HttpClientExtensionsClassPath(projectDirectory, solutionName, "").ClassDirectory);
                HttpClientExtensionsBuilder.Create(projectDirectory, solutionName);
            }
        }
    }
}
