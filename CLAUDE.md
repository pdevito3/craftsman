# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### Build Commands
```bash
dotnet restore                          # Restore dependencies
dotnet build                           # Build the project
dotnet build --configuration Release   # Build for release
dotnet pack                            # Package as NuGet tool
```

### Running the Tool
```bash
dotnet run                             # Run the craftsman tool
craftsman new example MyProject       # Generate example project (when installed globally)
```

### Installation
```bash
dotnet tool install -g craftsman       # Install as global tool
```

## Architecture Overview

Craftsman is a .NET 8 scaffolding tool that generates clean architecture .NET Web APIs. The codebase follows a command-pattern architecture using Spectre.Console.Cli for the CLI interface.

### Core Architecture Components

**Commands** (`/Commands/`): CLI command implementations
- `NewDomainCommand` - Scaffolds complete domain projects
- `NewExampleCommand` - Generates example projects via CLI prompts
- `AddEntityCommand` - Adds entities to existing projects
- `AddFeatureCommand` - Adds features using CLI prompts
- Other Add/Register commands for bounded contexts, auth servers, consumers, producers

**Builders** (`/Builders/`): Code generation classes that create specific file types
- Entity builders (entities, DTOs, mappings)
- Endpoint builders (controllers, CRUD operations)
- Test builders (unit, integration, functional tests)
- Infrastructure builders (database, services, Docker)
- NextJS builders for frontend scaffolding

**Domain** (`/Domain/`): Template definitions and configuration models
- Template classes define the structure of generated projects
- Entity, API, and message templates
- Configuration models for database providers, authentication, etc.

**Services** (`/Services/`): Core scaffolding services
- `ApiScaffoldingService` - Main API generation orchestration
- `EntityScaffoldingService` - Entity-specific scaffolding
- `NextJsEntityScaffoldingService` - Frontend scaffolding
- `GitService` - Git repository management
- `ClassPathHelper` - File path management utilities

### Generated Project Architecture

Craftsman generates projects using:
- **Clean Architecture** with vertical slice organization
- **CQRS + MediatR** for feature organization  
- **Entity Framework Core** with multiple database provider support
- **Docker** containerization
- **Integration/Unit/Functional** test scaffolding
- **Mass Transit** for message bus integration
- **Duende Auth Server** integration

### Key Patterns

- **Builder Pattern**: All code generation uses dedicated builder classes
- **Template-driven**: Uses template files (YAML/JSON) to define project structure
- **Dependency Injection**: Uses Microsoft.Extensions.DependencyInjection throughout
- **Command Pattern**: CLI commands implement Spectre.Console command pattern
- **Service Registration**: Auto-discovery of services implementing `ICraftsmanService`

### File Organization

- Builders are organized by the type of code they generate (Tests/, Docker/, Auth/, etc.)
- Each builder typically generates one specific file type or closely related files
- Template classes in Domain/ define the data structures for generated projects
- Command classes orchestrate multiple builders to create complete features

The tool's primary function is translating YAML/JSON configuration files into complete .NET Web API projects with accompanying tests, Docker configuration, and optional frontend scaffolding.

### Conventions

- Raw string literals should be used for file text and include a `// lang=csharp` marker. For example:

  ```csharp
          private static string GetFileText(string classNamespace, string entityName)
          {
              // lang=csharp
              return $$"""
                       namespace {{classNamespace}};
  
                       public sealed class {{FileNames.EntityCreatedDomainMessage(entityName)}} : DomainEvent
                       {
                           public {{entityName}} {{entityName}} { get; set; } 
                       }
                                   
                       """;
          }
  ```

  

- When creating a `Builder` or `Modifier`, be sure to set it up as a MediatR command like with DI in the handler for `IScaffoldingDirectoryStore`  or whatever else is needed. Here is an example:

  ```c#
  public static class CreatedDomainEventBuilder
  {
      public class CreatedDomainEventBuilderCommand(string entityName, string entityPlural) : IRequest<bool>
      {
          public string EntityName { get; set; } = entityName;
          public string EntityPlural { get; set; } = entityPlural;
      }
  
      public class Handler(
          ICraftsmanUtilities utilities,
          IScaffoldingDirectoryStore scaffoldingDirectoryStore)
          : IRequestHandler<CreatedDomainEventBuilderCommand, bool>
      {
          public Task<bool> Handle(CreatedDomainEventBuilderCommand request, CancellationToken cancellationToken)
          {
              var classPath = ClassPathHelper.DomainEventsClassPath(scaffoldingDirectoryStore.SrcDirectory,
                  $"{FileNames.EntityCreatedDomainMessage(request.EntityName)}.cs",
                  request.EntityPlural,
                  scaffoldingDirectoryStore.ProjectBaseName);
              var fileText = GetFileText(classPath.ClassNamespace, request.EntityName);
              utilities.CreateFile(classPath, fileText);
              return Task.FromResult(true);
          }
  
          private static string GetFileText(string classNamespace, string entityName)
          {
              // lang=csharp
              return $$"""
                       namespace {{classNamespace}};
  
                       public sealed class {{FileNames.EntityCreatedDomainMessage(entityName)}} : DomainEvent
                       {
                           public {{entityName}} {{entityName}} { get; set; } 
                       }
                                   
                       """;
          }
      }
  }
  ```

  

- When a new feature is added, bug is fixed, etc. be sure to add the updates to the Changelog

