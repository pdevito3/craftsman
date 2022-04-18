namespace NewCraftsman.Builders;

using System;
using System.IO;
using System.IO.Abstractions;
using Helpers;
using Projects;
using Services;

public class SolutionBuilder
{
    private readonly IFileSystem _fileSystem;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IConsoleWriter _console;

    public SolutionBuilder(IFileSystem fileSystem, ICraftsmanUtilities utilities, IConsoleWriter console)
    {
        _fileSystem = fileSystem;
        _utilities = utilities;
        _console = console;
    }
    
    public void BuildSolution(string solutionDirectory, string projectName, IFileSystem fileSystem)
    {
        fileSystem.Directory.CreateDirectory(solutionDirectory);
        _utilities.ExecuteProcess("dotnet", @$"new sln -n {projectName}", solutionDirectory);
        BuildSharedKernelProject(solutionDirectory, fileSystem);
    }

    public void AddProjects(string solutionDirectory, string srcDirectory, string testDirectory, string dbProvider, string dbName, string projectBaseName, bool addJwtAuth, IFileSystem fileSystem)
    {
        // add webapi first so it is default project
        BuildWebApiProject(solutionDirectory, srcDirectory, projectBaseName, addJwtAuth, dbProvider, dbName, fileSystem);
        BuildIntegrationTestProject(solutionDirectory, testDirectory, projectBaseName, addJwtAuth);
        BuildFunctionalTestProject(solutionDirectory, testDirectory, projectBaseName, addJwtAuth);
        BuildSharedTestProject(solutionDirectory, testDirectory, projectBaseName);
        BuildUnitTestProject(solutionDirectory, testDirectory, projectBaseName);
    }

    private void BuildWebApiProject(string solutionDirectory, string srcDirectory, string projectBaseName, bool useJwtAuth, string dbProvider, string dbName, IFileSystem fileSystem)
    {
        var solutionFolder = srcDirectory.GetSolutionFolder(solutionDirectory);
        var webApiProjectClassPath = ClassPathHelper.WebApiProjectClassPath(srcDirectory, projectBaseName);

        WebApiCsProjBuilder.CreateWebApiCsProj(srcDirectory, projectBaseName, dbProvider);
        _utilities.ExecuteProcess("dotnet", $@"sln add ""{webApiProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

        // base folders
        Directory.CreateDirectory(ClassPathHelper.ControllerClassPath(srcDirectory, "", projectBaseName, "v1").ClassDirectory);
        Directory.CreateDirectory(ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, "", projectBaseName).ClassDirectory);
        Directory.CreateDirectory(ClassPathHelper.WebApiMiddlewareClassPath(srcDirectory, "", projectBaseName).ClassDirectory);

        // additional from what was other projects
        Directory.CreateDirectory(ClassPathHelper.DtoClassPath(solutionDirectory, "", "", projectBaseName).ClassDirectory);
        Directory.CreateDirectory(ClassPathHelper.ExceptionsClassPath(solutionDirectory, "").ClassDirectory);
        Directory.CreateDirectory(ClassPathHelper.WrappersClassPath(srcDirectory, "", projectBaseName).ClassDirectory);
        Directory.CreateDirectory(ClassPathHelper.SharedDtoClassPath(solutionDirectory, "").ClassDirectory);
        Directory.CreateDirectory(ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName).ClassDirectory);
        Directory.CreateDirectory(ClassPathHelper.DummySeederClassPath(srcDirectory, "", projectBaseName).ClassDirectory);

        WebApiServiceExtensionsBuilder.CreateApiVersioningServiceExtension(srcDirectory, projectBaseName, fileSystem);
        WebApiServiceExtensionsBuilder.CreateCorsServiceExtension(srcDirectory, projectBaseName, fileSystem);
        WebApiServiceExtensionsBuilder.CreateWebApiServiceExtension(srcDirectory, projectBaseName, fileSystem);
        OpenTelemetryExtensionsBuilder.CreateOTelServiceExtension(srcDirectory, projectBaseName, dbProvider, fileSystem);
        ErrorHandlerFilterAttributeBuilder.CreateErrorHandlerFilterAttribute(srcDirectory, projectBaseName, fileSystem);
        AppSettingsBuilder.CreateWebApiAppSettings(srcDirectory, dbName, projectBaseName);
        WebApiLaunchSettingsBuilder.CreateLaunchSettings(srcDirectory, projectBaseName, fileSystem);
        ProgramBuilder.CreateWebApiProgram(srcDirectory, projectBaseName, fileSystem);
        StartupBuilder.CreateWebApiStartup(srcDirectory, useJwtAuth, projectBaseName, fileSystem);
        LocalConfigBuilder.CreateLocalConfig(srcDirectory, projectBaseName, fileSystem);
        LoggingConfigurationBuilder.CreateWebApiConfigFile(srcDirectory, projectBaseName, fileSystem);
        InfrastructureServiceRegistrationBuilder.CreateInfrastructureServiceExtension(srcDirectory, projectBaseName, fileSystem);
        
        BasePaginationParametersBuilder.CreateBasePaginationParameters(solutionDirectory, projectBaseName, fileSystem);
        PagedListBuilder.CreatePagedList(srcDirectory, projectBaseName, fileSystem);
        CoreExceptionsBuilder.CreateExceptions(solutionDirectory, projectBaseName, fileSystem);
        
        _utilities.AddProjectReference(webApiProjectClassPath, @"..\..\..\SharedKernel\SharedKernel.csproj");
    }

    private void BuildIntegrationTestProject(string solutionDirectory, string testDirectory, string projectBaseName, bool addJwtAuth)
    {
        var solutionFolder = testDirectory.GetSolutionFolder(solutionDirectory);
        var testProjectClassPath = ClassPathHelper.IntegrationTestProjectRootClassPath(testDirectory, "", projectBaseName);

        IntegrationTestsCsProjBuilder.CreateTestsCsProj(testDirectory, projectBaseName, addJwtAuth);
        _utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
    }

    private void BuildFunctionalTestProject(string solutionDirectory, string testDirectory, string projectBaseName, bool addJwtAuth)
    {
        var solutionFolder = testDirectory.GetSolutionFolder(solutionDirectory);
        var testProjectClassPath = ClassPathHelper.FunctionalTestProjectRootClassPath(testDirectory, "", projectBaseName);

        FunctionalTestsCsProjBuilder.CreateTestsCsProj(testDirectory, projectBaseName, addJwtAuth);
        _utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
    }

    private void BuildSharedTestProject(string solutionDirectory, string testDirectory, string projectBaseName)
    {
        var solutionFolder = testDirectory.GetSolutionFolder(solutionDirectory);
        var testProjectClassPath = ClassPathHelper.SharedTestProjectRootClassPath(testDirectory, "", projectBaseName);

        SharedTestsCsProjBuilder.CreateTestsCsProj(testDirectory, projectBaseName);
        _utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
    }

    private void BuildUnitTestProject(string solutionDirectory, string testDirectory, string projectBaseName)
    {
        var solutionFolder = testDirectory.GetSolutionFolder(solutionDirectory);
        var testProjectClassPath = ClassPathHelper.UnitTestProjectRootClassPath(testDirectory, "", projectBaseName);

        UnitTestsCsProjBuilder.CreateTestsCsProj(testDirectory, projectBaseName);
        _utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
    }

    public void BuildSharedKernelProject(string solutionDirectory, IFileSystem fileSystem)
    {
        var projectExists = File.Exists(Path.Combine(solutionDirectory, "SharedKernel", "SharedKernel.csproj"));
        if (projectExists) return;
        
        var projectClassPath = ClassPathHelper.SharedKernelProjectRootClassPath(solutionDirectory, "");
        SharedKernelCsProjBuilder.CreateMessagesCsProj(solutionDirectory, fileSystem);
        _utilities.ExecuteProcess("dotnet", $@"sln add ""{projectClassPath.FullClassPath}""", solutionDirectory);
    }

    public void BuildAuthServerProject(string solutionDirectory, string authServerProjectName, IFileSystem fileSystem)
    {
        var projectExists = File.Exists(Path.Combine(solutionDirectory, authServerProjectName, $"{authServerProjectName}.csproj"));
        if (projectExists) return;
        
        var projectClassPath = ClassPathHelper.AuthServerProjectClassPath(solutionDirectory, authServerProjectName);
        AuthServerProjBuilder.CreateProject(solutionDirectory, authServerProjectName, fileSystem);
        _utilities.ExecuteProcess("dotnet", $@"sln add ""{projectClassPath.FullClassPath}""", solutionDirectory);
    }

    public void BuildBffProject(string solutionDirectory, string projectName, int? proxyPort, IFileSystem fileSystem)
    {
        var projectExists = File.Exists(Path.Combine(solutionDirectory, projectName, $"{projectName}.csproj"));
        if (projectExists) return;
        
        var projectClassPath = ClassPathHelper.BffProjectClassPath(solutionDirectory, projectName);
        BffProjBuilder.CreateProject(solutionDirectory, projectName, proxyPort, fileSystem);
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