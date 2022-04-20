namespace NewCraftsman.Builders;

using System;
using System.IO;
using System.IO.Abstractions;
using Domain;
using ExtensionBuilders;
using Helpers;
using Projects;
using Services;

public class SolutionBuilder
{
    private readonly IFileSystem _fileSystem;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IConsoleWriter _consoleWriter;

    public SolutionBuilder(IFileSystem fileSystem, ICraftsmanUtilities utilities, IConsoleWriter consoleWriter)
    {
        _fileSystem = fileSystem;
        _utilities = utilities;
        _consoleWriter = consoleWriter;
    }
    
    public void BuildSolution(string solutionDirectory, string projectName)
    {
        _fileSystem.Directory.CreateDirectory(solutionDirectory);
        _utilities.ExecuteProcess("dotnet", @$"new sln -n {projectName}", solutionDirectory);
        BuildSharedKernelProject(solutionDirectory);
    }

    public void AddProjects(string solutionDirectory, string srcDirectory, string testDirectory, DbProvider dbProvider, string dbName, string projectBaseName, bool addJwtAuth)
    {
        // add webapi first so it is default project
        BuildWebApiProject(solutionDirectory, srcDirectory, projectBaseName, addJwtAuth, dbProvider, dbName);
        BuildIntegrationTestProject(solutionDirectory, testDirectory, projectBaseName);
        BuildFunctionalTestProject(solutionDirectory, testDirectory, projectBaseName);
        BuildSharedTestProject(solutionDirectory, testDirectory, projectBaseName);
        BuildUnitTestProject(solutionDirectory, testDirectory, projectBaseName);
    }

    private void BuildWebApiProject(string solutionDirectory, string srcDirectory, string projectBaseName, bool useJwtAuth, DbProvider dbProvider, string dbName)
    {
        var solutionFolder = srcDirectory.GetSolutionFolder(solutionDirectory);
        var webApiProjectClassPath = ClassPathHelper.WebApiProjectClassPath(srcDirectory, projectBaseName);

        new WebApiCsProjBuilder(_utilities).CreateWebApiCsProj(srcDirectory, projectBaseName, dbProvider);
        _utilities.ExecuteProcess("dotnet", $@"sln add ""{webApiProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

        // base folders
        _fileSystem.Directory.CreateDirectory(ClassPathHelper.ControllerClassPath(srcDirectory, "", projectBaseName, "v1").ClassDirectory);
        _fileSystem.Directory.CreateDirectory(ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, "", projectBaseName).ClassDirectory);
        _fileSystem.Directory.CreateDirectory(ClassPathHelper.WebApiMiddlewareClassPath(srcDirectory, "", projectBaseName).ClassDirectory);

        // additional from what was other projects
        _fileSystem.Directory.CreateDirectory(ClassPathHelper.DtoClassPath(solutionDirectory, "", "", projectBaseName).ClassDirectory);
        _fileSystem.Directory.CreateDirectory(ClassPathHelper.ExceptionsClassPath(solutionDirectory, "").ClassDirectory);
        _fileSystem.Directory.CreateDirectory(ClassPathHelper.WrappersClassPath(srcDirectory, "", projectBaseName).ClassDirectory);
        _fileSystem.Directory.CreateDirectory(ClassPathHelper.SharedDtoClassPath(solutionDirectory, "").ClassDirectory);
        _fileSystem.Directory.CreateDirectory(ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName).ClassDirectory);
        _fileSystem.Directory.CreateDirectory(ClassPathHelper.DummySeederClassPath(srcDirectory, "", projectBaseName).ClassDirectory);

        new ApiVersioningExtensionsBuilder(_utilities).CreateApiVersioningServiceExtension(srcDirectory, projectBaseName);
        new CorsExtensionsBuilder(_utilities).CreateCorsServiceExtension(srcDirectory, projectBaseName);
        new WebApiServiceExtensionsBuilder(_utilities).CreateWebApiServiceExtension(srcDirectory, projectBaseName);
        new OpenTelemetryExtensionsBuilder(_utilities).CreateOTelServiceExtension(srcDirectory, projectBaseName, dbProvider);
        // ErrorHandlerFilterAttributeBuilder.CreateErrorHandlerFilterAttribute(srcDirectory, projectBaseName, _fileSystem);
        // AppSettingsBuilder.CreateWebApiAppSettings(srcDirectory, dbName, projectBaseName);
        // WebApiLaunchSettingsBuilder.CreateLaunchSettings(srcDirectory, projectBaseName, _fileSystem);
        // ProgramBuilder.CreateWebApiProgram(srcDirectory, projectBaseName, _fileSystem);
        // StartupBuilder.CreateWebApiStartup(srcDirectory, useJwtAuth, projectBaseName, _fileSystem);
        // LocalConfigBuilder.CreateLocalConfig(srcDirectory, projectBaseName, _fileSystem);
        // LoggingConfigurationBuilder.CreateWebApiConfigFile(srcDirectory, projectBaseName, _fileSystem);
        // InfrastructureServiceRegistrationBuilder.CreateInfrastructureServiceExtension(srcDirectory, projectBaseName, _fileSystem);
        //
        // BasePaginationParametersBuilder.CreateBasePaginationParameters(solutionDirectory, projectBaseName, _fileSystem);
        // PagedListBuilder.CreatePagedList(srcDirectory, projectBaseName, _fileSystem);
        // CoreExceptionsBuilder.CreateExceptions(solutionDirectory, projectBaseName, _fileSystem);
        
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

    // public void BuildAuthServerProject(string solutionDirectory, string authServerProjectName)
    // {
    //     var projectExists = File.Exists(Path.Combine(solutionDirectory, authServerProjectName, $"{authServerProjectName}.csproj"));
    //     if (projectExists) return;
    //     
    //     var projectClassPath = ClassPathHelper.AuthServerProjectClassPath(solutionDirectory, authServerProjectName);
    //     AuthServerProjBuilder.CreateProject(solutionDirectory, authServerProjectName, _fileSystem);
    //     _utilities.ExecuteProcess("dotnet", $@"sln add ""{projectClassPath.FullClassPath}""", solutionDirectory);
    // }
    //
    // public void BuildBffProject(string solutionDirectory, string projectName, int? proxyPort)
    // {
    //     var projectExists = File.Exists(Path.Combine(solutionDirectory, projectName, $"{projectName}.csproj"));
    //     if (projectExists) return;
    //     
    //     var projectClassPath = ClassPathHelper.BffProjectClassPath(solutionDirectory, projectName);
    //     BffProjBuilder.CreateProject(solutionDirectory, projectName, proxyPort, _fileSystem);
    //     _utilities.ExecuteProcess("dotnet", $@"sln add ""{projectClassPath.FullClassPath}""", solutionDirectory);
    // }
}

public static class Extensions
{
    public static string GetSolutionFolder(this string projectDir, string solutionDir)
    {
        var folder = projectDir.Replace(solutionDir, "");

        return folder.Length > 0 ? folder.Substring(1) : folder;
    }
}