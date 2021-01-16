namespace Craftsman.Builders
{
    using Craftsman.Builders.Dtos;
    using Craftsman.Builders.Projects;
    using Craftsman.Builders.Tests.IntegrationTests;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using static Helpers.ConsoleWriter;

    public class SolutionBuilder
    {
        public static void BuildSolution(string solutionDirectory, ApiTemplate template, IFileSystem fileSystem)
        {
            try
            {
                fileSystem.Directory.CreateDirectory(solutionDirectory);
                Utilities.ExecuteProcess("dotnet", @$"new sln -n {template.SolutionName}", solutionDirectory);
            }
            catch(Exception e)
            {
                WriteError(e.Message);
                throw;

                // custom error that you must be using the .net 5 sdk?
            }
        }

        public static void AddProjects(string solutionDirectory, string dbProvider, string soutionName, IFileSystem fileSystem, List<ApplicationUser> inMemoryUsers)
        {
            // add webapi first so it is default project
            BuildWebApiProject(solutionDirectory, fileSystem, inMemoryUsers);
            BuildDomainProject(solutionDirectory);
            BuildApplicationProject(solutionDirectory, fileSystem);
            BuildInfrastructurePersistenceProject(solutionDirectory, dbProvider, fileSystem);
            BuildInfrastructureSharedProject(solutionDirectory, fileSystem);
            BuildTestProject(solutionDirectory, soutionName);
        }

        private static void BuildDomainProject(string solutionDirectory)
        {
            var domainProjectClassPath = ClassPathHelper.DomainProjectClassPath(solutionDirectory);

            DomainCsProjBuilder.CreateDomainCsProj(solutionDirectory);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{domainProjectClassPath.FullClassPath}"" --solution-folder {"Core"}", solutionDirectory);

            // dir won't show up in project until file is added
            Directory.CreateDirectory(ClassPathHelper.EntityClassPath(solutionDirectory, "").ClassDirectory);
        }

        private static void BuildApplicationProject(string solutionDirectory, IFileSystem fileSystem)
        {
            var applicationProjectClassPath = ClassPathHelper.ApplicationProjectClassPath(solutionDirectory);

            ApplicationCsProjBuilder.CreateApplicationCsProj(solutionDirectory);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{applicationProjectClassPath.FullClassPath}"" --solution-folder {"Core"}", solutionDirectory);

            // base folders
            Directory.CreateDirectory(ClassPathHelper.DtoClassPath(solutionDirectory,"","").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.ApplicationExceptionClassPath(solutionDirectory, "").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.ApplicationInterfaceClassPath(solutionDirectory, "").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.ProfileClassPath(solutionDirectory, "").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.ValidationClassPath(solutionDirectory, "", "").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.WrappersClassPath(solutionDirectory, "").ClassDirectory);

            Directory.CreateDirectory(ClassPathHelper.SharedDtoClassPath(solutionDirectory, "").ClassDirectory);

            ApplicationServiceExtensionsBuilder.CreateApplicationServiceExtension(solutionDirectory, fileSystem);
            BasePaginationParametersBuilder.CreateBasePaginationParameters(solutionDirectory, fileSystem);
            PagedListBuilder.CreatePagedList(solutionDirectory, fileSystem);
            ResponseBuilder.CreateResponse(solutionDirectory, fileSystem);
            ApplicationExceptionsBuilder.CreateExceptions(solutionDirectory);
        }

        private static void BuildInfrastructurePersistenceProject(string solutionDirectory, string dbProvider, IFileSystem fileSystem)
        {
            var infrastructurePersistenceProjectClassPath = ClassPathHelper.InfrastructurePersistenceProjectClassPath(solutionDirectory);

            InfrastructurePersistenceCsProjBuilder.CreateInfrastructurePersistenceCsProj(solutionDirectory, dbProvider);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{infrastructurePersistenceProjectClassPath.FullClassPath}"" --solution-folder {"Infrastructure"}", solutionDirectory);

            // base folders
            Directory.CreateDirectory(ClassPathHelper.DbContextClassPath(solutionDirectory, "").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.RepositoryClassPath(solutionDirectory, "").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.SeederClassPath(solutionDirectory, "").ClassDirectory);

            InfrastructurePersistenceServiceRegistrationBuilder.CreateInfrastructurePersistenceServiceExtension(solutionDirectory, fileSystem);
        }

        private static void BuildInfrastructureSharedProject(string solutionDirectory, IFileSystem fileSystem)
        {
            var infrastructureSharedProjectClassPath = ClassPathHelper.InfrastructureSharedProjectClassPath(solutionDirectory);

            InfrastructureSharedCsProjBuilder.CreateInfrastructureSharedCsProj(solutionDirectory);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{infrastructureSharedProjectClassPath.FullClassPath}"" --solution-folder {"Infrastructure"}", solutionDirectory);

            InfrastructureSharedServiceRegistrationBuilder.CreateInfrastructureSharedServiceExtension(solutionDirectory, fileSystem);
        }

        private static void BuildWebApiProject(string solutionDirectory, IFileSystem fileSystem, List<ApplicationUser> inMemoryUsers)
        {
            var webApiProjectClassPath = ClassPathHelper.WebApiProjectClassPath(solutionDirectory);

            WebApiCsProjBuilder.CreateWebApiCsProj(solutionDirectory);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{webApiProjectClassPath.FullClassPath}"" --solution-folder {"Api"}", solutionDirectory);

            // base folders
            Directory.CreateDirectory(ClassPathHelper.ControllerClassPath(solutionDirectory, "").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.WebApiExtensionsClassPath(solutionDirectory, "").ClassDirectory);
            Directory.CreateDirectory(ClassPathHelper.WebApiMiddlewareClassPath(solutionDirectory, "").ClassDirectory);

            WebApiServiceExtensionsBuilder.CreateWebApiServiceExtension(solutionDirectory, fileSystem);
            WebApiAppExtensionsBuilder.CreateWebApiAppExtension(solutionDirectory, fileSystem);
            ErrorHandlerMiddlewareBuilder.CreateErrorHandlerMiddleware(solutionDirectory, fileSystem);
            AppSettingsBuilder.CreateAppSettings(solutionDirectory);
            LaunchSettingsBuilder.CreateLaunchSettings(solutionDirectory, fileSystem);
            WebApiProgramBuilder.CreateWebApiProgram(solutionDirectory, fileSystem);
            StartupBuilder.CreateStartup(solutionDirectory, "Startup", null, inMemoryUsers);
        }

        private static void BuildTestProject(string solutionDirectory, string solutionName)
        {
            var testProjectClassPath = ClassPathHelper.TestProjectRootClassPath(solutionDirectory, "", solutionName);

            TestsCsProjBuilder.CreateTestsCsProj(solutionDirectory, solutionName);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {"Tests"}", solutionDirectory);

            HealthCheckTestBuilder.CreateHealthCheckTests(solutionDirectory, solutionName);
        }
    }
}
