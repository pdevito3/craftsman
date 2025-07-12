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

public static class SolutionBuilder
{
    public sealed record BuildSolutionCommand(string SolutionDirectory, string ProjectName) : IRequest;
    public sealed record AddProjectsCommand(string SolutionDirectory, string SrcDirectory, string TestDirectory, DbProvider DbProvider, string ProjectBaseName, bool AddJwtAuth, int OtelAgentPort, bool UseCustomErrorHandler) : IRequest;
    public sealed record BuildSharedKernelProjectCommand(string SolutionDirectory) : IRequest;
    public sealed record BuildAuthServerProjectCommand(string SolutionDirectory, string AuthServerProjectName) : IRequest;
    public sealed record BuildBffProjectCommand(string SolutionDirectory, string ProjectName, int? ProxyPort) : IRequest;

    public class BuildSolutionHandler(
        ICraftsmanUtilities utilities,
        IFileSystem fileSystem,
        IMediator mediator)
        : IRequestHandler<BuildSolutionCommand>
    {
        public async Task Handle(BuildSolutionCommand request, CancellationToken cancellationToken)
        {
            fileSystem.Directory.CreateDirectory(request.SolutionDirectory);
            utilities.ExecuteProcess("dotnet", @$"new sln -n {request.ProjectName}", request.SolutionDirectory);
            await mediator.Send(new BuildSharedKernelProjectCommand(request.SolutionDirectory), cancellationToken);
        }
    }

    public class AddProjectsHandler(
        ICraftsmanUtilities utilities,
        IFileSystem fileSystem,
        IMediator mediator)
        : IRequestHandler<AddProjectsCommand>
    {
        public async Task Handle(AddProjectsCommand request, CancellationToken cancellationToken)
        {
            // add webapi first so it is default project
            await BuildWebApiProject(request.SolutionDirectory, request.SrcDirectory, request.ProjectBaseName, request.AddJwtAuth, request.DbProvider, request.OtelAgentPort, request.UseCustomErrorHandler);
            await BuildIntegrationTestProject(request.SolutionDirectory, request.TestDirectory, request.ProjectBaseName, request.DbProvider);
            BuildFunctionalTestProject(request.SolutionDirectory, request.TestDirectory, request.ProjectBaseName, request.DbProvider);
            BuildSharedTestProject(request.SolutionDirectory, request.TestDirectory, request.ProjectBaseName);
            BuildUnitTestProject(request.SolutionDirectory, request.TestDirectory, request.ProjectBaseName);
        }

    private async Task BuildWebApiProject(string solutionDirectory, string srcDirectory, string projectBaseName, bool useJwtAuth, DbProvider dbProvider, int otelAgentPort, bool useCustomErrorHandler)
    {
        var solutionFolder = srcDirectory.GetSolutionFolder(solutionDirectory);
        var webApiProjectClassPath = ClassPathHelper.WebApiProjectClassPath(srcDirectory, projectBaseName);

        await mediator.Send(new WebApiCsProjBuilder.WebApiCsProjBuilderCommand(dbProvider, useCustomErrorHandler));
        utilities.ExecuteProcess("dotnet", $@"sln add ""{webApiProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);

        // base folders
        fileSystem.Directory.CreateDirectory(ClassPathHelper.ControllerClassPath(srcDirectory, "", projectBaseName, "v1").ClassDirectory);
        fileSystem.Directory.CreateDirectory(ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, "", projectBaseName).ClassDirectory);
        fileSystem.Directory.CreateDirectory(ClassPathHelper.WebApiMiddlewareClassPath(srcDirectory, "", projectBaseName).ClassDirectory);

        // additional from what was other projects
        fileSystem.Directory.CreateDirectory(ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName).ClassDirectory);
        fileSystem.Directory.CreateDirectory(ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "", projectBaseName).ClassDirectory);
        fileSystem.Directory.CreateDirectory(ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "", projectBaseName).ClassDirectory);
        fileSystem.Directory.CreateDirectory(ClassPathHelper.DbContextClassPath(srcDirectory, "", projectBaseName).ClassDirectory);

        await mediator.Send(new ApiVersioningExtensionsBuilder.ApiVersioningExtensionsBuilderCommand());
        await mediator.Send(new CorsExtensionsBuilder.CorsExtensionsBuilderCommand());
        await mediator.Send(new OpenTelemetryExtensionsBuilder.OpenTelemetryExtensionsBuilderCommand(dbProvider, otelAgentPort));
        await mediator.Send(new WebApiLaunchSettingsBuilder.WebApiLaunchSettingsBuilderCommand());
        await mediator.Send(new ProgramBuilder.ProgramBuilderCommand(useJwtAuth, useCustomErrorHandler));
        await mediator.Send(new ServiceConfigurationBuilder.ServiceConfigurationBuilderCommand(useCustomErrorHandler));
        await mediator.Send(new ConstsResourceBuilder.ConstsResourceBuilderCommand());
        await mediator.Send(new QueryKitConfigBuilder.QueryKitConfigBuilderCommand());
        await mediator.Send(new InfrastructureServiceRegistrationBuilder.InfrastructureServiceRegistrationBuilderCommand(srcDirectory, projectBaseName));

        if (useCustomErrorHandler)
        {
            await mediator.Send(new ErrorHandlerFilterAttributeBuilder.ErrorHandlerFilterAttributeBuilderCommand());
        }
        else
        {
            await mediator.Send(new ErrorHandlerWithHellang.Command());
        }

        await mediator.Send(new BasePaginationParametersBuilder.BasePaginationParametersBuilderCommand());
        await mediator.Send(new PagedListBuilder.PagedListBuilderCommand());
        await mediator.Send(new CoreExceptionBuilder.CoreExceptionBuilderCommand());

        utilities.AddProjectReference(webApiProjectClassPath, @"..\..\..\SharedKernel\SharedKernel.csproj");
    }

    private async Task BuildIntegrationTestProject(string solutionDirectory, string testDirectory, string projectBaseName, DbProvider dbProvider)
    {
        var solutionFolder = testDirectory.GetSolutionFolder(solutionDirectory);
        var testProjectClassPath = ClassPathHelper.IntegrationTestProjectRootClassPath(testDirectory, "", projectBaseName);

        await mediator.Send(new IntegrationTestsCsProjBuilder.IntegrationTestsCsProjBuilderCommand(dbProvider));
        utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
    }

    private void BuildFunctionalTestProject(string solutionDirectory, string testDirectory, string projectBaseName, DbProvider dbProvider)
    {
        var solutionFolder = testDirectory.GetSolutionFolder(solutionDirectory);
        var testProjectClassPath = ClassPathHelper.FunctionalTestProjectRootClassPath(testDirectory, "", projectBaseName);

        mediator.Send(new FunctionalTestsCsProjBuilder.Command(dbProvider)).GetAwaiter().GetResult();
        utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
    }

    private void BuildSharedTestProject(string solutionDirectory, string testDirectory, string projectBaseName)
    {
        var solutionFolder = testDirectory.GetSolutionFolder(solutionDirectory);
        var testProjectClassPath = ClassPathHelper.SharedTestProjectRootClassPath(testDirectory, "", projectBaseName);

        mediator.Send(new SharedTestsCsProjBuilder.Command()).GetAwaiter().GetResult();
        utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
    }

    private void BuildUnitTestProject(string solutionDirectory, string testDirectory, string projectBaseName)
    {
        var solutionFolder = testDirectory.GetSolutionFolder(solutionDirectory);
        var testProjectClassPath = ClassPathHelper.UnitTestProjectRootClassPath(testDirectory, "", projectBaseName);

        mediator.Send(new UnitTestsCsProjBuilder.Command()).GetAwaiter().GetResult();
        utilities.ExecuteProcess("dotnet", $@"sln add ""{testProjectClassPath.FullClassPath}"" --solution-folder {solutionFolder}", solutionDirectory);
    }
    }

    public class BuildSharedKernelProjectHandler(
        ICraftsmanUtilities utilities,
        IMediator mediator)
        : IRequestHandler<BuildSharedKernelProjectCommand>
    {
        public async Task Handle(BuildSharedKernelProjectCommand request, CancellationToken cancellationToken)
        {
            var projectExists = File.Exists(Path.Combine(request.SolutionDirectory, "SharedKernel", "SharedKernel.csproj"));
            if (projectExists) return;

            var projectClassPath = ClassPathHelper.SharedKernelProjectRootClassPath(request.SolutionDirectory, "");
            await mediator.Send(new SharedKernelCsProjBuilder.Command(), cancellationToken);
            utilities.ExecuteProcess("dotnet", $@"sln add ""{projectClassPath.FullClassPath}""", request.SolutionDirectory);
        }
    }

    public class BuildAuthServerProjectHandler(
        ICraftsmanUtilities utilities,
        IMediator mediator)
        : IRequestHandler<BuildAuthServerProjectCommand>
    {
        public async Task Handle(BuildAuthServerProjectCommand request, CancellationToken cancellationToken)
        {
            var projectExists = File.Exists(Path.Combine(request.SolutionDirectory, request.AuthServerProjectName, $"{request.AuthServerProjectName}.csproj"));
            if (projectExists) return;

            var projectClassPath = ClassPathHelper.AuthServerProjectClassPath(request.SolutionDirectory, request.AuthServerProjectName);
            await mediator.Send(new AuthServerProjBuilder.Command(request.AuthServerProjectName), cancellationToken);
            utilities.ExecuteProcess("dotnet", $@"sln add ""{projectClassPath.FullClassPath}""", request.SolutionDirectory);
        }
    }

    public class BuildBffProjectHandler(
        ICraftsmanUtilities utilities,
        IMediator mediator)
        : IRequestHandler<BuildBffProjectCommand>
    {
        public async Task Handle(BuildBffProjectCommand request, CancellationToken cancellationToken)
        {
            var projectExists = File.Exists(Path.Combine(request.SolutionDirectory, request.ProjectName, $"{request.ProjectName}.csproj"));
            if (projectExists) return;

            var projectClassPath = ClassPathHelper.BffProjectClassPath(request.SolutionDirectory, request.ProjectName);
            await mediator.Send(new BffProjBuilder.Command(request.ProjectName, request.ProxyPort), cancellationToken);
            utilities.ExecuteProcess("dotnet", $@"sln add ""{projectClassPath.FullClassPath}""", request.SolutionDirectory);
        }
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