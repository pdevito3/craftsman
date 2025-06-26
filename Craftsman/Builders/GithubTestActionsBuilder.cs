namespace Craftsman.Builders;

using Helpers;
using Humanizer;
using MediatR;
using Services;

public static class GithubTestActionsBuilder
{
    public sealed record CreateUnitTestActionCommand(string SolutionDirectory, string ProjectBaseName) : IRequest;
    public sealed record CreateIntegrationTestActionCommand(string SolutionDirectory, string ProjectBaseName) : IRequest;
    public sealed record CreateFunctionalTestActionCommand(string SolutionDirectory, string ProjectBaseName) : IRequest;

    public class CreateUnitTestActionHandler(ICraftsmanUtilities utilities) : IRequestHandler<CreateUnitTestActionCommand>
    {
        public Task Handle(CreateUnitTestActionCommand request, CancellationToken cancellationToken)
        {
            var humanized = $"{request.ProjectBaseName}UnitTests".Kebaberize();
            var classPath = ClassPathHelper.GithubWorkflowsClassPath(request.SolutionDirectory, $"{humanized}.yaml");
            var fileText = GetUnitTestFileText(request.ProjectBaseName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }
    
    public class CreateIntegrationTestActionHandler(ICraftsmanUtilities utilities) : IRequestHandler<CreateIntegrationTestActionCommand>
    {
        public Task Handle(CreateIntegrationTestActionCommand request, CancellationToken cancellationToken)
        {
            var humanized = $"{request.ProjectBaseName}IntegrationTests".Kebaberize();
            var classPath = ClassPathHelper.GithubWorkflowsClassPath(request.SolutionDirectory, $"{humanized}.yaml");
            var fileText = GetIntegrationTestFileText(request.ProjectBaseName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }
    
    public class CreateFunctionalTestActionHandler(ICraftsmanUtilities utilities) : IRequestHandler<CreateFunctionalTestActionCommand>
    {
        public Task Handle(CreateFunctionalTestActionCommand request, CancellationToken cancellationToken)
        {
            var humanized = $"{request.ProjectBaseName}FunctionalTests".Kebaberize();
            var classPath = ClassPathHelper.GithubWorkflowsClassPath(request.SolutionDirectory, $"{humanized}.yaml");
            var fileText = GetFunctionalTestFileText(request.ProjectBaseName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    public static string GetUnitTestFileText(string projectBaseName)
    {
        return @$"name: Unit Tests

on: [pull_request, workflow_dispatch]

jobs:
  test:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        dotnet-version: ['8.0.x']
    steps:
      - uses: actions/checkout@v2
      - name: Setup .NET Core SDK ${{{{ matrix.dotnet-version }}}}
        uses: actions/setup-dotnet@v1.7.2
        with:
          dotnet-version: ${{{{ matrix.dotnet-version }}}}

      - name: Install dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --configuration Release --no-restore
      - name: Test
        working-directory: {projectBaseName}/tests/{projectBaseName}.UnitTests
        run: dotnet test --no-restore --verbosity minimal";
    }

    public static string GetIntegrationTestFileText(string projectBaseName)
    {
        return @$"name: Integration Tests

on: [pull_request, workflow_dispatch]

jobs:
  test:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        dotnet-version: ['8.0.x']
    steps:
      - uses: actions/checkout@v2
      - name: Setup .NET Core SDK ${{{{ matrix.dotnet-version }}}}
        uses: actions/setup-dotnet@v1.7.2
        with:
          dotnet-version: ${{{{ matrix.dotnet-version }}}}

      - name: Install dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --configuration Release --no-restore
      - name: Test
        working-directory: {projectBaseName}/tests/{projectBaseName}.IntegrationTests
        run: dotnet test --no-restore --verbosity minimal";
    }

    public static string GetFunctionalTestFileText(string projectBaseName)
    {
        return @$"name: Functional Tests

on: [pull_request, workflow_dispatch]

jobs:
  test:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        dotnet-version: ['8.0.x']
    steps:
      - uses: actions/checkout@v2
      - name: Setup .NET Core SDK ${{{{ matrix.dotnet-version }}}}
        uses: actions/setup-dotnet@v1.7.2
        with:
          dotnet-version: ${{{{ matrix.dotnet-version }}}}

      - name: Install dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --configuration Release --no-restore
      - name: Test
        working-directory: {projectBaseName}/tests/{projectBaseName}.FunctionalTests
        run: dotnet test --no-restore --verbosity minimal";
    }
}
