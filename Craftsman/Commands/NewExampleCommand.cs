namespace Craftsman.Commands;

using System.IO.Abstractions;
using Builders;
using Domain;
using Helpers;
using MediatR;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;

public class NewExampleCommand : Command<NewExampleCommand.Settings>
{
    private readonly IAnsiConsole _console;
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;
    private readonly IDbMigrator _dbMigrator;
    private readonly IGitService _gitService;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;
    private readonly IFileParsingHelper _fileParsingHelper;
    private readonly IMediator _mediator;

    public NewExampleCommand(IAnsiConsole console, IFileSystem fileSystem, IConsoleWriter consoleWriter, ICraftsmanUtilities utilities, IScaffoldingDirectoryStore scaffoldingDirectoryStore, IDbMigrator dbMigrator, IGitService gitService, IFileParsingHelper fileParsingHelper, IMediator mediator)
    {
        _console = console;
        _fileSystem = fileSystem;
        _consoleWriter = consoleWriter;
        _utilities = utilities;
        _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        _dbMigrator = dbMigrator;
        _gitService = gitService;
        _fileParsingHelper = fileParsingHelper;
        _mediator = mediator;
    }

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[ProjectName]")]
        public string ProjectName { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var rootDir = _fileSystem.Directory.GetCurrentDirectory();
        var myEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (myEnv == "Dev")
            rootDir = _console.Ask<string>("Enter the root directory of your project:");

        var (exampleType, projectName) = RunPrompt(settings.ProjectName);
        var templateString = GetExampleDomain(projectName, exampleType);

        var domainProject = FileParsingHelper.ReadYamlString<DomainProject>(templateString);

        _scaffoldingDirectoryStore.SetSolutionDirectory(rootDir, domainProject.DomainName);
        var domainCommand = new NewDomainCommand(_console, _fileSystem, _consoleWriter, _utilities, _scaffoldingDirectoryStore, _dbMigrator, _gitService, _fileParsingHelper, _mediator);
        domainCommand.CreateNewDomainProject(domainProject);

        new ExampleTemplateBuilder(_utilities).CreateYamlFile(_scaffoldingDirectoryStore.SolutionDirectory,
            templateString);
        _console.MarkupLine($"{Environment.NewLine}[bold yellow1]Your example project is project is ready![/]");

        _consoleWriter.StarGithubRequest();
        return 0;
    }

    private (ExampleType type, string name) RunPrompt(string projectName)
    {
        _console.WriteLine();
        _console.Write(new Rule("[yellow]Create an Example Project[/]").RuleStyle("grey").Centered());

        var typeString = AskExampleType();
        var exampleType = ExampleType.FromName(typeString, ignoreCase: true);
        if (string.IsNullOrEmpty(projectName))
            projectName = AskExampleProjectName();

        return (exampleType, projectName);
    }

    private string AskExampleType()
    {
        var exampleTypes = ExampleType.List.Select(e => e.Name);

        return _console.Prompt(
            new SelectionPrompt<string>()
                .Title("What [green]type of example[/] do you want to create?")
                .PageSize(50)
                .AddChoices(exampleTypes)
        );
    }

    private string AskExampleProjectName()
    {
        return _console.Ask<string>("What would you like to name this project (e.g. [green]MyExampleProject[/])?");
    }

    private static string GetExampleDomain(string name, ExampleType exampleType)
    {
        if (exampleType == ExampleType.Basic)
            return BasicTemplate(name);
        if (exampleType == ExampleType.WithAuth)
            return AuthTemplate(name);
        if (exampleType == ExampleType.WithBus)
            return BusTemplate(name);
        if (exampleType == ExampleType.WithAuthServer)
            return AuthServerTemplate(name);
        if (exampleType == ExampleType.WithForeignKey)
            return ForeignKeyTemplate(name);
        if (exampleType == ExampleType.Complex)
            return ComplexTemplate(name);

        throw new Exception("Example type was not recognized.");
    }

    private static string ForeignKeyTemplate(string name)
    {
        return $@"DomainName: {name}
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
    - Name: Author
      Type: Author
      ForeignEntityName: Author
      ForeignEntityPlural: Authors
    - Name: Ingredients
      Type: ICollection<Ingredient>
      ForeignEntityPlural: Ingredients
  - Name: Author
    Features:
    - Type: GetList
    - Type: GetRecord
    - Type: AddRecord
    - Type: UpdateRecord
    - Type: DeleteRecord
    Properties:
    - Name: Name
      Type: string
      CanFilter: true
      CanSort: true
    - Name: RecipeId
      Type: Guid
      ForeignEntityName: Recipe
      ForeignEntityPlural: Recipes
  - Name: Ingredient
    Features:
    - Type: GetList
    - Type: GetRecord
    - Type: AddRecord
    - Type: UpdateRecord
    - Type: DeleteRecord
    - Type: AddListByFk
      BatchPropertyName: RecipeId
      BatchPropertyType: Guid
      ParentEntity: Recipe
      ParentEntityPlural: Recipes
    Properties:
    - Name: Name
      Type: string
      CanFilter: true
      CanSort: true
    - Name: Visibility
      SmartNames:
      - Public
      - Friends Only
      - Private
      CanFilter: true
      CanSort: true
    - Name: Quantity
      Type: string
      CanFilter: true
      CanSort: true
    - Name: Measure
      Type: string
      CanFilter: true
      CanSort: true
    - Name: RecipeId
      Type: Guid
      ForeignEntityName: Recipe";
    }

    private static string ComplexTemplate(string name)
    {
        return $@"DomainName: {name}
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
      IsProtected: true
      PermissionName: CanReadRecipes
    - Type: GetRecord
      IsProtected: true
      PermissionName: CanReadRecipes
    - Type: AddRecord
      IsProtected: true
    - Type: UpdateRecord
      IsProtected: true
    - Type: DeleteRecord
      IsProtected: true
    Properties:
    - Name: Title
      Type: string
      CanFilter: true
      CanSort: true
    - Name: Visibility
      SmartNames:
      - Public
      - Friends Only
      - Private
      CanFilter: true
      CanSort: true
    - Name: Directions
      Type: string
      CanFilter: true
      CanSort: true
    - Name: Rating
      Type: int?
      CanFilter: true
      CanSort: true
    - Name: DateOfOrigin
      Type: DateOnly?
      CanFilter: true
      CanSort: true
    - Name: HaveMadeItMyself
      Type: bool
      CanFilter: true
      CanSort: true
    - Name: Author
      Type: Author
      ForeignEntityName: Author
      ForeignEntityPlural: Authors
    - Name: Ingredients
      Type: ICollection<Ingredient>
      ForeignEntityPlural: Ingredients
  - Name: Author
    Features:
    - Type: GetList
    - Type: GetRecord
    - Type: AddRecord
    - Type: UpdateRecord
    - Type: DeleteRecord
    Properties:
    - Name: Name
      Type: string
      CanFilter: true
      CanSort: true
    - Name: RecipeId
      Type: Guid
      ForeignEntityName: Recipe
      ForeignEntityPlural: Recipes
  - Name: Ingredient
    Features:
    - Type: GetList
    - Type: GetRecord
    - Type: AddRecord
    - Type: UpdateRecord
    - Type: DeleteRecord
    - Type: AddListByFk
      BatchPropertyName: RecipeId
      BatchPropertyType: Guid
      ParentEntity: Recipe
      ParentEntityPlural: Recipes
    Properties:
    - Name: Name
      Type: string
      CanFilter: true
      CanSort: true
    - Name: Quantity
      Type: string
      CanFilter: true
      CanSort: true
    - Name: ExpiresOn
      Type: DateTime?
      CanFilter: true
      CanSort: true
    - Name: Measure
      Type: string
      CanFilter: true
      CanSort: true
    - Name: RecipeId
      Type: Guid
      ForeignEntityName: Recipe
  Environment:
      AuthSettings:
        Authority: http://localhost:3255/auth/realms/DevRealm
        Audience: recipe_management
        AuthorizationUrl: http://localhost:3255/auth/realms/DevRealm/protocol/openid-connect/auth
        TokenUrl: http://localhost:3255/auth/realms/DevRealm/protocol/openid-connect/token
        ClientId: recipe_management.swagger
        ClientSecret: 974d6f71-d41b-4601-9a7a-a33081f80687
      BrokerSettings:
        Host: localhost
        VirtualHost: /
        Username: guest
        Password: guest
  Bus:
    AddBus: true
  Producers:
  - EndpointRegistrationMethodName: AddRecipeProducerEndpoint
    ProducerName: AddRecipeProducer
    ExchangeName: recipe-added
    MessageName: RecipeAdded
    DomainDirectory: Recipes
    ExchangeType: fanout
    UsesDb: true
  Consumers:
  - EndpointRegistrationMethodName: AddToBookEndpoint
    ConsumerName: AddToBook
    ExchangeName: book-additions
    QueueName: add-recipe-to-book
    MessageName: RecipeAdded
    DomainDirectory: Recipes
    ExchangeType: fanout
Messages:
- Name: RecipeAdded
  Properties:
  - Name: RecipeId
    Type: guid
Bff:
  ProjectName: RecipeManagementApp
  ProxyPort: 4378
  HeadTitle: Recipe Management App
  Authority: http://localhost:3255/auth/realms/DevRealm
  ClientId: recipe_management.bff
  ClientSecret: 974d6f71-d41b-4601-9a7a-a33081f80688
  RemoteEndpoints:
    - LocalPath: /api/recipes
      ApiAddress: https://localhost:5375/api/recipes
    - LocalPath: /api/ingredients
      ApiAddress: https://localhost:5375/api/ingredients
  BoundaryScopes:
    - recipe_management
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
      Type: string #optional if string
    - Name: Directions
    - Name: RecipeSourceLink
    - Name: Visibility
    - Name: Description
    - Name: Rating
      Type: number?
    - Name: DateOfOrigin
      Type: Date
    - Name: HaveMadeItMyself
      Type: boolean
  - Name: Ingredient
    Features:
    - Type: GetList
    - Type: GetRecord
    - Type: AddRecord
    - Type: UpdateRecord
    - Type: DeleteRecord
    Properties:
    - Name: Name
    - Name: Quantity
    - Name: Measure
    - Name: RecipeId
AuthServer:
  Name: KeycloakPulumi
  RealmName: DevRealm
  Port: 3255
  Clients:
    - Id: recipe_management.postman.machine
      Name: RecipeManagement Postman Machine
      Secret: 974d6f71-d41b-4601-9a7a-a33081f84682
      GrantType: ClientCredentials
      BaseUrl: 'https://oauth.pstmn.io/'
      Scopes:
        - recipe_management #this should match the audience scope in your boundary auth settings and swagger specs
    - Id: recipe_management.postman.code
      Name: RecipeManagement Postman Code
      Secret: 974d6f71-d41b-4601-9a7a-a33081f84680 #optional
      GrantType: Code
      BaseUrl: 'https://oauth.pstmn.io/'
      Scopes:
        - recipe_management #this should match the audience scope in your boundary auth settings and swagger specs
    - Id: recipe_management.swagger
      Name: RecipeManagement Swagger
      Secret: 974d6f71-d41b-4601-9a7a-a33081f80687
      GrantType: Code
      BaseUrl: 'https://localhost:5375/'
      Scopes:
        - recipe_management #this should match the audience scope in your boundary auth settings and swagger specs
    - Id: recipe_management.bff
      Name: RecipeManagement BFF
      Secret: 974d6f71-d41b-4601-9a7a-a33081f80688
      BaseUrl: 'https://localhost:4378/'
      GrantType: Code
      RedirectUris:
        - 'https://localhost:4378/*'
      AllowedCorsOrigins:
        - 'https://localhost:5375' # api 1 - recipe_management
        - 'https://localhost:4378'
      Scopes:
        - recipe_management #this should match the audience scope in your boundary auth settings and swagger specs";
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
   Provider: postgres
   NamingConvention: class
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
    - Name: Visibility
      SmartNames:
      - Public
      - Friends Only
      - Private
      CanFilter: true
      CanSort: true
    - Name: DateOfOrigin
      Type: DateOnly?
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
    Provider: postgres
  Entities:
  - Name: Recipe
    Features:
    - Type: GetList
      IsProtected: true
      PermissionName: CanReadRecipes
    - Type: GetRecord
      IsProtected: true
      PermissionName: CanReadRecipes
    - Type: AddRecord
      IsProtected: true
    - Type: UpdateRecord
      IsProtected: true
    - Type: DeleteRecord
      IsProtected: true
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
    - Name: Visibility
      SmartNames:
      - Public
      - Friends Only
      - Private
      CanFilter: true
      CanSort: true
    - Name: DateOfOrigin
      Type: DateOnly?
      CanFilter: true
      CanSort: true
  Environment:
    AuthSettings:
      Authority: http://localhost:3255/auth/realms/DevRealm
      Audience: recipe_management
      AuthorizationUrl: http://localhost:3255/auth/realms/DevRealm/protocol/openid-connect/auth
      TokenUrl: http://localhost:3255/auth/realms/DevRealm/protocol/openid-connect/token
      ClientId: recipe_management.swagger
      ClientSecret: 974d6f71-d41b-4601-9a7a-a33081f80687";
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
    - Name: Visibility
      SmartNames:
      - Public
      - Friends Only
      - Private
      CanFilter: true
      CanSort: true
    - Name: DateOfOrigin
      Type: DateOnly?
      CanFilter: true
      CanSort: true
  Environment:
      BrokerSettings:
        Host: localhost
        VirtualHost: /
        Username: guest
        Password: guest
  Bus:
    AddBus: true
  Producers:
  - EndpointRegistrationMethodName: AddRecipeProducerEndpoint
    ProducerName: AddRecipeProducer
    ExchangeName: recipe-added
    MessageName: RecipeAdded
    DomainDirectory: Recipes
    ExchangeType: fanout
    UsesDb: true
  Consumers:
  - EndpointRegistrationMethodName: AddToBookEndpoint
    ConsumerName: AddToBook
    ExchangeName: book-additions
    QueueName: add-recipe-to-book
    MessageName: RecipeAdded
    DomainDirectory: Recipes
    ExchangeType: fanout
Messages:
- Name: RecipeAdded
  Properties:
  - Name: RecipeId
    Type: guid";

        return template;
    }

    private static string AuthServerTemplate(string name)
    {
        return $@"DomainName: {name}
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
      IsProtected: true
      PermissionName: CanReadRecipes
    - Type: GetRecord
      IsProtected: true
      PermissionName: CanReadRecipes
    - Type: AddRecord
      IsProtected: true
    - Type: UpdateRecord
      IsProtected: true
    - Type: DeleteRecord
      IsProtected: true
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
    - Name: Rating
      Type: int?
      CanFilter: true
      CanSort: true
    - Name: Visibility
      SmartNames:
      - Public
      - Friends Only
      - Private
      CanFilter: true
      CanSort: true
    - Name: DateOfOrigin
      Type: DateOnly?
      CanFilter: true
      CanSort: true
  Environment:
    AuthSettings:
      Authority: http://localhost:3255/auth/realms/DevRealm
      Audience: recipe_management
      AuthorizationUrl: http://localhost:3255/auth/realms/DevRealm/protocol/openid-connect/auth
      TokenUrl: http://localhost:3255/auth/realms/DevRealm/protocol/openid-connect/token
      ClientId: recipe_management.swagger
      ClientSecret: 974d6f71-d41b-4601-9a7a-a33081f80687
Bff:
  ProjectName: RecipeManagementApp
  ProxyPort: 4378
  HeadTitle: Recipe Management App
  Authority: https://localhost:3385
  ClientId: recipe_management.bff
  ClientSecret: 974d6f71-d41b-4601-9a7a-a33081f80687
  RemoteEndpoints:
    - LocalPath: /api/recipes
      ApiAddress: https://localhost:5375/api/recipes
  BoundaryScopes:
    - recipe_management
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
      Type: string #optional if string
    - Name: Directions
    - Name: RecipeSourceLink
    - Name: Visibility
    - Name: Description
    - Name: ImageLink
    - Name: Visibility
    - Name: Rating
      Type: number?
AuthServer:
  Name: KeycloakPulumi
  RealmName: DevRealm
  Port: 3255
  Clients:
    - Id: recipe_management.postman.machine
      Name: RecipeManagement Postman Machine
      Secret: 974d6f71-d41b-4601-9a7a-a33081f84682
      GrantType: ClientCredentials
      BaseUrl: 'https://oauth.pstmn.io/'
      Scopes:
        - recipe_management #this should match the audience scope in your boundary auth settings and swagger specs
    - Id: recipe_management.postman.code
      Name: RecipeManagement Postman Code
      Secret: 974d6f71-d41b-4601-9a7a-a33081f84680 #optional
      GrantType: Code
      BaseUrl: 'https://oauth.pstmn.io/'
      Scopes:
        - recipe_management #this should match the audience scope in your boundary auth settings and swagger specs
    - Id: recipe_management.swagger
      Name: RecipeManagement Swagger
      Secret: 974d6f71-d41b-4601-9a7a-a33081f80687
      GrantType: Code
      BaseUrl: 'https://localhost:5375/'
      Scopes:
        - recipe_management #this should match the audience scope in your boundary auth settings and swagger specs
    - Id: recipe_management.bff
      Name: RecipeManagement BFF
      Secret: 974d6f71-d41b-4601-9a7a-a33081f80688
      BaseUrl: 'https://localhost:4378/'
      GrantType: Code
      RedirectUris:
        - 'https://localhost:4378/*'
      AllowedCorsOrigins:
        - 'https://localhost:5375' # api 1 - recipe_management
        - 'https://localhost:4378'
      Scopes:
        - recipe_management #this should match the audience scope in your boundary auth settings and swagger specs
";
    }
}