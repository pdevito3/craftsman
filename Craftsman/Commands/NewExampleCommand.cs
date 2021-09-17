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
            // if(exampleType == ExampleType.WithBus)
            //     return GetWithBusProject(name);

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

        private static DomainProject GetWithBusProject(string name)
        {
            var boundary = new ApiTemplate();
            
            boundary.Environments = new List<ApiEnvironment>()
            {
                new ApiEnvironment()
                {
                    EnvironmentName = "Development",
                    BrokerSettings =
                    {
                        Host = "localhost",
                        VirtualHost = "/",
                        Username = "guest",
                        Password = "guest",
                    }
                }
            };
            boundary.Bus.AddBus = true;
            boundary.Producers = new List<Producer>()
            {
                new Producer()
                {
                    EndpointRegistrationMethodName = "EmailRequestor",
                    ExchangeName = "report-requests",
                    MessageName = "ISendReportRequest",
                    ExchangeType = "direct",
                    ProducerName = "EmailWasRequested",
                    UsesDb = true,
                }
            };
            boundary.Consumers = new List<Consumer>()
            {
                new()
                {
                    EndpointRegistrationMethodName = "EmailReportsEndpoint",
                    ConsumerName = "SendRequestedEmail",
                    ExchangeName = "report-requests",
                    MessageName = "ISendReportRequest",
                    QueueName = "email-reports",
                    ExchangeType = "direct",
                    RoutingKey = "email",
                    UsesDb = true,
                },
                new()
                {
                    EndpointRegistrationMethodName = "FaxReportsEndpoint",
                    ConsumerName = "SendRequestedFax",
                    ExchangeName = "report-requests",
                    MessageName = "ISendReportRequest",
                    QueueName = "fax-reports",
                    ExchangeType = "direct",
                    RoutingKey = "fax",
                    IsLazy = false,
                    IsQuorum = false,
                    UsesDb = false,
                }
            };
            var messages = new List<Message>()
            {
                new Message()
                {
                    Name = "ISendReportRequest",
                    Properties = new List<MessageProperty>()
                    {
                        new MessageProperty()
                        {
                            Name = "ReportId",
                            Type = "guid"
                        },
                        new MessageProperty()
                        {
                            Name = "Provider",
                            Type = "string"
                        },
                        new MessageProperty()
                        {
                            Name = "Target",
                            Type = "string"
                        }
                    }
                }
            };

            var entity = new Entity()
            {
                Name = "ReportRequest",
                Properties = new List<EntityProperty>()
                {
                    new EntityProperty()
                    {
                        Name = "Provider",
                        Type = "string"
                    },
                    new EntityProperty()
                    {
                        Name = "Target",
                        Type = "string"
                    }
                }
            };
            
            boundary.Entities.Clear();
            boundary.Entities.Add(entity);
            boundary.ProjectName = "ReportManagement";
            boundary.DbContext = new TemplateDbContext()
            {
                ContextName = "ReportManagementDbContext",
                DatabaseName = "ReportManagement",
                Provider = "Postgres"
            };
            
            
            var template = new DomainProject()
            {
                DomainName = name,
                BoundedContexts = new List<ApiTemplate>() {boundary},
                Messages = messages
            };

            return template;
        }
    }
}