namespace Craftsman.Commands
{
    using Craftsman.Builders;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using Spectre.Console;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading.Tasks;
    using CommandLine.Text;
    using static Helpers.ConsoleWriter;

    public static class NewExampleCommand
    {
        public static void Help()
        {
            WriteHelpHeader(@$"Description:");
            WriteHelpText(@$"   Scaffolds out an example project.{Environment.NewLine}");

            WriteHelpHeader(@$"Usage:");
            WriteHelpText(@$"   craftsman new:example [options] arguments{Environment.NewLine}");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Options:");
            WriteHelpText(@$"   -h, --help          Display this help message.");

            WriteHelpText(Environment.NewLine);
            WriteHelpHeader(@$"Example:");
            WriteHelpText(@$"   craftsman new:example --type basic --name MyExampleProjectName");
            WriteHelpText(@$"   craftsman new:example --type withauth --name MyExampleProjectName");
            WriteHelpText(@$"   craftsman new:example --type withbus --name MyExampleProjectName{Environment.NewLine}");
        }

        public static void Run(string buildSolutionDirectory, IFileSystem fileSystem)
        {
            try
            {
                var promptResponse = RunPrompt();
                var templateString = GetExampleDomain(promptResponse.name, promptResponse.type);
                
                var domainProject = FileParsingHelper.ReadYamlString<DomainProject>(templateString);
                var domainDirectory = $"{buildSolutionDirectory}{Path.DirectorySeparatorChar}{domainProject.DomainName}";
                
                NewDomainProjectCommand.CreateNewDomainProject(domainDirectory, fileSystem, domainProject);
                ExampleTemplateBuilder.CreateYamlFile(domainDirectory, templateString, fileSystem);

                AnsiConsole.MarkupLine($"{Environment.NewLine}[bold yellow1]Your example project is project is ready![/]");
                StarGithubRequest();
            }
            catch (Exception e)
            {
                if (e is FileAlreadyExistsException
                    || e is DirectoryAlreadyExistsException
                    || e is InvalidSolutionNameException
                    || e is FileNotFoundException
                    || e is InvalidDbProviderException
                    || e is InvalidFileTypeException
                    || e is DataValidationErrorException
                    || e is SolutiuonNameEntityMatchException)
                {
                    WriteError($"{e.Message}");
                }
                else
                {
                    AnsiConsole.WriteException(e, new ExceptionSettings
                    {
                        Format = ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks,
                        Style = new ExceptionStyle
                        {
                            Exception = new Style().Foreground(Color.Grey),
                            Message = new Style().Foreground(Color.White),
                            NonEmphasized = new Style().Foreground(Color.Cornsilk1),
                            Parenthesis = new Style().Foreground(Color.Cornsilk1),
                            Method = new Style().Foreground(Color.Red),
                            ParameterName = new Style().Foreground(Color.Cornsilk1),
                            ParameterType = new Style().Foreground(Color.Red),
                            Path = new Style().Foreground(Color.Red),
                            LineNumber = new Style().Foreground(Color.Cornsilk1),
                        }
                    });
                }
            }
        }
        private static (ExampleType type, string name) RunPrompt()
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Render(new Rule("[yellow]Create an Example Project[/]").RuleStyle("grey").Centered());

            var typeString = AskExampleType();
            var exampleType = ExampleType.FromName(typeString, ignoreCase: true);
            var projectName = AskExampleProjectName();

            return (exampleType, projectName);
        }

        private static string AskExampleType()
        {
            var exampleTypes = ExampleType.List.Select(e => e.Name);
            
            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What [green]type of example[/] do you want to create?")
                    .PageSize(50)
                    .AddChoices(exampleTypes)
            );
        }

        private static string AskExampleProjectName()
        {
            return AnsiConsole.Ask<string>("What would you like to name this project (e.g. [green]MyExampleProject[/])?");
        }
        
        private static string GetExampleDomain(string name, ExampleType exampleType)
        {
            if (exampleType == ExampleType.Basic)
                return BasicTemplate(name);
            if (exampleType == ExampleType.WithAuth)
                return AuthTemplate(name);
            if(exampleType == ExampleType.WithBus)
                return BusTemplate(name);

            throw new Exception("Example type was not recognized.");
        }

        private static string BasicTemplate(string name)
        {
            return $@"DomainName: {name}
BoundedContexts:
- ProjectName: RecipeManagement
  Port: 5375
  DbContext:
   ContextName: RecipesDbContext
   DatabaseName: RecipeManagement
   Provider: SqlServer
  Entities:
  - Name: Recipe
    Features:
    - Type: GetList
    - Type: GetRecord
    - Type: AddRecord
    - Type: UpdateRecord
    - Type: DeleteRecord
    Properties:
    - Name: Title
      Type: string
      CanFilter: true
      CanSort: true
    - Name: Directions
      Type: string
      CanFilter: true
      CanSort: true
    - Name: RecipeSourceLink
      Type: string
      CanFilter: true
      CanSort: true
    - Name: Description
      Type: string
      CanFilter: true
      CanSort: true
    - Name: ImageLink
      Type: string
      CanFilter: true
      CanSort: true";
        }
        
        private static string AuthTemplate(string name)
        {
            return $@"DomainName: {name}
BoundedContexts:
- ProjectName: RecipeManagement
  Port: 5375
  DbContext:
    ContextName: RecipesDbContext
    DatabaseName: RecipeManagement
    Provider: SqlServer
  Entities:
  - Name: Recipe
    Features:
    - Type: GetList
      Policies:
      - Name: CanReadRecipes
        PolicyType: scope
        PolicyValue: recipes.read
    - Type: GetRecord
      Policies:
      - Name: CanReadRecipes
        PolicyType: scope
        PolicyValue: recipes.read
    - Type: AddRecord
      Policies:
      - Name: CanAddRecipes
        PolicyType: scope
        PolicyValue: recipes.add
    - Type: UpdateRecord
      Policies:
      - Name: CanUpdateRecipes
        PolicyType: scope
        PolicyValue: recipes.update
    - Type: DeleteRecord
      Policies:
      - Name: CanDeleteRecipes
        PolicyType: scope
        PolicyValue: recipes.delete
    Properties:
    - Name: Title
      Type: string
      CanFilter: true
      CanSort: true
    - Name: Directions
      Type: string
      CanFilter: true
      CanSort: true
    - Name: RecipeSourceLink
      Type: string
      CanFilter: true
      CanSort: true
    - Name: Description
      Type: string
      CanFilter: true
      CanSort: true
    - Name: ImageLink
      Type: string
      CanFilter: true
      CanSort: true
  Environments:
    - EnvironmentName: Development
      Authority: https://localhost:5010
      Audience: recipeManagementDev
      AuthorizationUrl: https://localhost:5010/connect/authorize
      TokenUrl: https://localhost:5010/connect/token
      ClientId: service.client.dev
    - EnvironmentName: Qa
      ConnectionString: ""MyQaConnectionString""
      Authority: https://qaauth.com
      Audience: recipeManagementQa
      AuthorizationUrl: https://qaauth.com/connect/authorize
      TokenUrl: https://qaauth.com/connect/token
      ClientId: service.client.qa
    - EnvironmentName: Production
      ConnectionString: ""MyProdConnectionString""
      Authority: https://auth.com
      Audience: recipeManagement
      AuthorizationUrl: https://auth.com/connect/authorize
      TokenUrl: https://auth.com/connect/token
      ClientId: service.client";
        }

        private static string BusTemplate(string name)
        {
            var template = $@"DomainName: {name}
BoundedContexts:
- ProjectName: RecipeManagement
  Port: 5375
  DbContext:
   ContextName: RecipesDbContext
   DatabaseName: RecipeManagement
   Provider: Postgres
  Entities:
  - Name: Recipe
    Features:
    - Type: GetList
    - Type: GetRecord
    - Type: AddRecord
    - Type: UpdateRecord
    - Type: DeleteRecord
    Properties:
    - Name: Title
      Type: string
      CanFilter: true
      CanSort: true
    - Name: Directions
      Type: string
      CanFilter: true
      CanSort: true
    - Name: RecipeSourceLink
      Type: string
      CanFilter: true
      CanSort: true
    - Name: Description
      Type: string
      CanFilter: true
      CanSort: true
    - Name: ImageLink
      Type: string
      CanFilter: true
      CanSort: true
  Environments:
    - EnvironmentName: Development
      Host localhost
      VirtualHost /
      Username guest
      Password guest
  AddBus: true
  Producers:
  - EndpointRegistrationMethodName: RecipeAddedEndpoint
    ProducerName: RecipeAdded
    ExchangeName: recipe-added
    MessageName: IRecipeAdded
    ExchangeType: fanout
    UsesDb: true
  Consumers:
  - EndpointRegistrationMethodName: AddToBookEndpoint
    ConsumerName: AddToBook
    ExchangeName: book-additions
    QueueName: add-recipe-to-book
    MessageName: IRecipeAdded
    ExchangeType: fanout
Messages:
- Name: IRecipeAdded
  Properties:
  - Name: RecipeId
  - Type: guid";

            return template;
        }
    }
}
