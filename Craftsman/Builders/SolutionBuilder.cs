namespace Craftsman.Builders;

using System.IO;
using System.IO.Abstractions;
using Domain;
using Dtos;
using ExtensionBuilders;
using Helpers;
using MediatR;
using Projects;
using Services;

public class SolutionBuilder
{
    private readonly IFileSystem _fileSystem;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IMediator _mediator;

    public SolutionBuilder(ICraftsmanUtilities utilities, IFileSystem fileSystem, IMediator mediator)
    {
        _fileSystem = fileSystem;
        _mediator = mediator;
        _utilities = utilities;
    }

    public void BuildSolution(string solutionDirectory, string projectName)
    {
        _fileSystem.Directory.CreateDirectory(solutionDirectory);
        _utilities.ExecuteProcess("dotnet", @$"new sln -n {projectName}", solutionDirectory);
        BuildSharedKernelProject(solutionDirectory);
    }

    public void AddProjects(string solutionDirectory, string srcDirectory, string testDirectory, DbProvider dbProvider, string projectBaseName, bool addJwtAuth, int otelAgentPort, bool useCustomErrorHandler)
    {
        // add webapi first so it is default project
        BuildWebApiProject(solutionDirectory, srcDirectory, projectBaseName, addJwtAuth, dbProvider, otelAgentPort, useCustomErrorHandler);
        BuildIntegrationTestProject(solutionDirectory, testDirectory, projectBaseName);
        BuildFunctionalTestProject(solutionDirectory, testDirectory, projectBaseName);
        BuildSharedTestProject(solutionDirectory, testDirectory, projectBaseName);
        BuildUnitTestProject(solutionDirectory, testDirectory, projectBaseName);
    }

    private void BuildWebApiProject(string solutionDirectory, string srcDirectory, string projectBaseName, bool useJwtAuth, DbProvider dbProvider, int otelAgentPort, bool useCustomErrorHandler)
    {
        var solutionFolder = srcDirectory.GetSolutionFolder(solutionDirectory);
        var webApiProjectClassPath = ClassPathHelper.WebApiProjectClassPath(srcDirectory, projectBaseName);

        new WebApiCsProjBuilder(_utilities).CreateWebApiCsProj(srcDirectory, projectBaseName, dbProvider, useCustomErrorHandler);
        _utilities.ExecuteProcess("dotnet", $@"sln add ""{webApiProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

        // base folders
        _fileSystem.Directory.CreateDirectory(ClassPathHelper.ControllerClassPath(srcDirectory, "", projectBaseName, "v1").ClassDirectory);
        _fileSystem.Directory.CreateDirectory(ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, "", projectBaseName).ClassDirectory);
        _fileSystem.Directory.CreateDirectory(ClassPathHelper.WebApiMiddlewareClassPath(srcDirectory, "", projectBaseName).ClassDirectory);

        // additional from what was other projects
        _fileSystem.Directory.CreateDirectory(ClassPathHelper.ExceptionsClassPath(solutionDirectory, "").ClassDirectory);
        _fileSystem.Directory.CreateDirectory(ClassPathHelper.WrappersClassPath(srcDirectory, "", projectBaseName).ClassDirectory);
        _fileSystem.Directory.CreateDirectory(ClassPathHelper.SharedDtoClassPath(solutionDirectory, "").ClassDirectory);
        _fileSystem.Directory.CreateDirectory(ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName).ClassDirectory);

        new ApiVersioningExtensionsBuilder(_utilities).CreateApiVersioningServiceExtension(srcDirectory, projectBaseName);
        new CorsExtensionsBuilder(_utilities).CreateCorsServiceExtension(srcDirectory, projectBaseName);
        new OpenTelemetryExtensionsBuilder(_utilities).CreateOTelServiceExtension(srcDirectory, projectBaseName, dbProvider, otelAgentPort);
        new WebApiLaunchSettingsBuilder(_utilities).CreateLaunchSettings(srcDirectory, projectBaseName);
        new ProgramBuilder(_utilities).CreateWebApiProgram(srcDirectory, useJwtAuth, projectBaseName, useCustomErrorHandler);
        new ServiceConfigurationBuilder(_utilities).CreateWebAppServiceConfiguration(srcDirectory, projectBaseName, useCustomErrorHandler);
        new ConstsResourceBuilder(_utilities).CreateLocalConfig(srcDirectory, projectBaseName);
        new QueryKitConfigBuilder(_utilities).CreateConfig(srcDirectory, projectBaseName);
        new LoggingConfigurationBuilder(_utilities).CreateWebApiConfigFile(srcDirectory, projectBaseName);
        new InfrastructureServiceRegistrationBuilder(_utilities).CreateInfrastructureServiceExtension(srcDirectory, projectBaseName);

        if (useCustomErrorHandler)
        {
            new ErrorHandlerFilterAttributeBuilder(_utilities).CreateErrorHandlerFilterAttribute(srcDirectory, projectBaseName);
        }
        else
        {
            new ErrorHandlerWithHellang(_utilities).CreateErrorHandler(srcDirectory, projectBaseName);
        }

        new BasePaginationParametersBuilder(_utilities).CreateBasePaginationParameters(solutionDirectory);
        new PagedListBuilder(_utilities).CreatePagedList(srcDirectory, projectBaseName);
        _mediator.Send(new CoreExceptionBuilder.CoreExceptionBuilderCommand());

        _utilities.AddProjectReference(webApiProjectClassPath, @"..\..\..\SharedKernel\SharedKernel.csproj");
    }

    private void BuildIntegrationTestProject(string solutionDirectory, string testDirectory, string projectBaseName)
    {
        var solutionFolder = testDirectory.GetSolutionFolder(solutionDirectory);
        var testProjectClassPath = ClassPathHelper.IntegrationTestProjectRootClassPath(testDirectory, "", projectBaseName);

        new IntegrationTestsCsProjBuilder(_utilities).CreateTestsCsProj(testDirectory, projectBaseName);
        _utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
    }

    private void BuildFunctionalTestProject(string solutionDirectory, string testDirectory, string projectBaseName)
    {
        var solutionFolder = testDirectory.GetSolutionFolder(solutionDirectory);
        var testProjectClassPath = ClassPathHelper.FunctionalTestProjectRootClassPath(testDirectory, "", projectBaseName);

        new FunctionalTestsCsProjBuilder(_utilities).CreateTestsCsProj(testDirectory, projectBaseName);
        _utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
    }

    private void BuildSharedTestProject(string solutionDirectory, string testDirectory, string projectBaseName)
    {
        var solutionFolder = testDirectory.GetSolutionFolder(solutionDirectory);
        var testProjectClassPath = ClassPathHelper.SharedTestProjectRootClassPath(testDirectory, "", projectBaseName);

        new SharedTestsCsProjBuilder(_utilities).CreateTestsCsProj(testDirectory, projectBaseName);
        _utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
    }

    private void BuildUnitTestProject(string solutionDirectory, string testDirectory, string projectBaseName)
    {
        var solutionFolder = testDirectory.GetSolutionFolder(solutionDirectory);
        var testProjectClassPath = ClassPathHelper.UnitTestProjectRootClassPath(testDirectory, "", projectBaseName);

        new UnitTestsCsProjBuilder(_utilities).CreateTestsCsProj(testDirectory, projectBaseName);
        _utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
    }

    public void BuildSharedKernelProject(string solutionDirectory)
    {
        var projectExists = File.Exists(Path.Combine(solutionDirectory, "SharedKernel", "SharedKernel.csproj"));
        if (projectExists) return;

        var projectClassPath = ClassPathHelper.SharedKernelProjectRootClassPath(solutionDirectory, "");
        new SharedKernelCsProjBuilder(_utilities).CreateSharedKernelCsProj(solutionDirectory);
        _utilities.ExecuteProcess("dotnet", $@"sln add ""{projectClassPath.FullClassPath}""", solutionDirectory);
    }

    public void BuildAuthServerProject(string solutionDirectory, string authServerProjectName)
    {
        var projectExists = File.Exists(Path.Combine(solutionDirectory, authServerProjectName, $"{authServerProjectName}.csproj"));
        if (projectExists) return;

        var projectClassPath = ClassPathHelper.AuthServerProjectClassPath(solutionDirectory, authServerProjectName);
        new AuthServerProjBuilder(_utilities).CreateProject(solutionDirectory, authServerProjectName);
        _utilities.ExecuteProcess("dotnet", $@"sln add ""{projectClassPath.FullClassPath}""", solutionDirectory);
    }

    public void BuildBffProject(string solutionDirectory, string projectName, int? proxyPort)
    {
        var projectExists = File.Exists(Path.Combine(solutionDirectory, projectName, $"{projectName}.csproj"));
        if (projectExists) return;

        var projectClassPath = ClassPathHelper.BffProjectClassPath(solutionDirectory, projectName);
        new BffProjBuilder(_utilities).CreateProject(solutionDirectory, projectName, proxyPort);
        _utilities.ExecuteProcess("dotnet", $@"sln add ""{projectClassPath.FullClassPath}""", solutionDirectory);
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