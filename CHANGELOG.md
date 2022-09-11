# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
~~and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).~~

> Semantic versioning will come once I hit v1, but at this point each `0.x` release may have breaking changes as I iron out the kinks of the framework. 

## Unreleased

### Added

* Test Utility Mapper for Unit Tests

* New `User` and `UserRole` entities when using auth
  * Your auth server will still store users, but it's *only* role is now AuthN. The basic premise is still the same though

    * `Authorize` attributes on your controllers will guard for application level access by checking if the request has a valid JWT from your auth server

    * Feature and business level access can be handled in your MediatR handlers with HeimGuard, for example:

      ```c#
      await _heimGuard.HasPermissionAsync("RecipesFullAccess") 
                ? Ok()
                : Forbidden();
      
      // OR...  
      
      await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddRecipe);
      ```

    * As usual, this will use the `UserPolicyHandler` logic to check if a user meets the given criteria. The implementation of HeimGuard's `UserPolicyHandler`  in the scaffolding will still check the token to see who you are Authenticated as, but will get the user's roles based on the configuration for that authenticated user within the boundary using the new concepts and logic described below.

  * Your auth server will still store users and their metadata that we want for AuthN and the new `User` entitiy will capture user metadata to be easily accessed in your app. Say you want to send back a list of recipes and show who created them, this gives you that info without having to reach out to your auth server
    * Users in your boundary will key to users in your auth server using the `Identifier` property, which should generally be populated with the `NameIdentifier` on your token.
    * Eventual consistancy TBD
    * Redis cache TBD?

  * Roles will also not be dictated by your auth server anymore and will be stored in the context of your boundary (not the shared kernel anymore, though you could move them there if it makes sense for your setup). They are still restricted with a smart enum to `Super Admin` and `User`, but feel free to modify or extend these as you wish.

  * When starting your api for the first time, you will not have any `User` or `UserRole` entries. To add an initial root user, you just need to authenticate (e.g. swagger) and hit a protected endpoint. This is because, by default, the `UserPolicyHandler` will now check if you have any users and, if not, add the requesting user as a root user with `Super Admin` access.
    * You are welcome to change this logic if you wish, it is just an easy default to start with.

  * If you have multiple boundaries, you can make the call on how you manage things. By default each boundary will store it's own set of users and roles. This might be useful if say you want to manage users in your billing boundary separate from your inventory boundary, but you may want to combine them too. Feel free to consolidate this as you wish if desired.

  * Users can be managed at the `/users` endpoints. This includes adding and removing their roles as user roles have no standalone meaning without a user.

  * Updated functional tests to properly handle the token setup

* New `Email` Value Object. Used on `User` if you want to see it in use


### Updated

* Queries and Commands now just have a request class of `Query` or `Command` respectively
* Change keycloak image from `jboss` to `sleighzy` for Apple Silicon support
* Unit of Work return a `Task<int>`
* Delete and Update commands return response based on UoW response
* Extra Ef Tools package for SqlServer
* `Development` Cors has `AllowCredentials`
* Better DTO indentation
* Auditable fields are sortable and filterable
* `GetList` feature defaults to `-CreatedOn` sort order if none is given
* Better `DateOnly` serialization support
* `Newtonsoft` -> `System.Text.Json`
* `Update` feature calls update in repository
* Added `8632` to `NoWarn` list in API projects -- ` [CS8632] The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.`
* Better error handling message for incorrect directory with boundary based commands
* New props on complex example

### Fixed

* Db configs are added for all entities

### Removed

* Patch endpoint scaffolding

## [0.16.6] - 09/05/2022

### Fixed

* Bad Test Fixture with SqlServer (fixes #95)

## [0.16.5] - 08/18/2022

### Fixed

* Revert parallel functional tests causing unpredictable failures

## [0.16.5] - 08/18/2022

### Fixed

* Revert parallel functional tests causing unpredictable failures

## [0.16.4] - 08/09/2022

### Updated

* Batch add returns list

### Fixed

* Mapster service registration uses assembly (and is escaped for web app factory in functional tests)

## [0.16.3] - 08/08/2022

### Fixed

* Fix typo on BFF redirect example

## [0.16.2] - 08/07/2022

### Fixed

* Remove offline BFF scope
* Fix example redirect for BFF

## [0.16.1] - 08/06/2022

### Fixed

* Unit test for filtering and sorting will exclude smart enum properties

## [0.16.0] - 08/06/2022

### Added

* `ServiceCollectionServiceExtensions` in Integration `TextFixture` for easier service mocking with `ReplaceServiceWithSingletonMock`
* Add default database configuration abstraction for each entity along with db context usage. Includes value object examples
* Json serialization extension to better handle `DateOnly` and `TimeOnly` in swagger
* Common value object scaffolding (`Address`, `Percent`, `MonetaryAmount`)
  * Scaffolds Dtos for `Address`
  * Scaffolds mappers

* Port customization for RMQ broker and ui
* Added `ForbiddenAccessException` and `InvalidSmartEnumPropertyName` handling to `ErrorHandlerFilterAttribute`

### Updated

* Removed faulty producer assertion in integration tests for better performance

* Remove `.AddFluentValidation(cfg => {{ cfg.AutomaticValidationEnabled = false; }});` from registration as validation should be happening directly at domain level

* Integration and functional tests parallelized

* Integration tests updated to use `DotNet.Testcontainers` for simpler docker db setup
  
* Moved pagination, filter, and sort testing to unit tests

* `UserPolicyHandler` uses RolePermission repo and handles finding no roles

* Value Object cleanup

* Better migration warning on integration tests

* Move most `UserPolicyHandler` tests to unit tests

* Move `CurrentUserServiceTests` test dir

* Update `LocalConfig` to `Consts` for all constants in app (other than permissions and roles)

* Update `AutoBogus` to `AutobogusLifesupport` to support .NET 6 and UTC

* `RequireHttpsMetadata`  off in dev for BFF and boundaries

* `SuperAdmin` text to `Super Admin`

* `UserPolicyHandler` can accommodate realm roles for keycloak

* Moved permission guards to each feature handler (still using `Authorize` attribute on controllers for authenication check).

* Simplified return on batch add feature

* CLI updates for auth server and bff scaffolds

* Migrated mapper to use `Mapster` instead of `Automapper` for easier value object usage and better performance. To migrate your project

  * Update your mappers to look like this (almost identicial to `Automapper`:

    ```csharp
    public class AuthorMappings : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<AuthorDto, Author>()
                .TwoWays();
            config.NewConfig<AuthorForCreationDto, Author>()
                .TwoWays();
            config.NewConfig<AuthorForUpdateDto, Author>()
                .TwoWays();
        }
    }
    ```

  * Manaully do mapping in your entity factories (due to recurssive nature of the dtos and mapping happining in the factories)

  * Update usings to `using MapsterMapper;`

  * Your `ProjectTo` mappings should look like this:

    ```csharp
    var dtoCollection = appliedCollection
    		.ProjectToType<AuthorDto>();
    ```

    and import:

    ```
    using MapsterMapper;
    using Mapster;
    ```

  * Your registration should now be:

    ```csharp
    var config = TypeAdapterConfig.GlobalSettings;
    builder.Services.AddSingleton(config);
    builder.Services.AddScoped<IMapper, ServiceMapper>();
    ```

### Fixed

* Domain events captured and cleared in db context before running to prevent dups
* FKs use proper entity name for property (#90)

## [0.15.0] - 06/05/2022

### Added

* Open Telemetry and Jaeger tracing support

* `ValueObject` class scaffolding to `SharedKernel`
  
* Domain Event support for all `BaseEntities`. Automatically scaffolded for the `Create` and `Update` methods that call messages in a new `DomainEvents` directory under that entity. It works by adding messages to a new `DomainEvents` prop on each entity and publishing all messages on an EF save using MediatR. 

  * Unit tests are also scaffolded
  * To consume a message, you can use a MediatR `INotificationHandler` like normal. For example:
  
  ```c#
  namespace RecipeManagement.Domain.Authors.Features;
  
  using System.Reflection.Metadata;
  using DomainEvents;
  using MediatR;
  
  public class LogAuthor : INotificationHandler<AuthorAdded>
  {
      private readonly ILogger<LogAuthor> _logger;
  
      public LogAuthor(ILogger<LogAuthor> logger)
      {
          _logger = logger;
      }
  
      public async Task Handle(AuthorAdded notification, CancellationToken cancellationToken)
      {
          _logger.LogInformation("Author added: {AuthorName}", notification.Author.Name);
      }
  }
  ```
  
* Smart enum property support. Just add a property and give it a list of options using the `SmartNames` property and you're good to go. They will all be strings with smart enum backing. Attributes like filtering and sorting are supported

  ```yaml
  
    Entities:
    - Name: Recipe
      Features:
      #...
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
  ```

  


### Updated

* CLI commands no longer use `:` and use the more traditional space delimiter. For exmaple, `craftsman new example`

* Moved from old Startup model to new Program only for .NET 6. Includes updating test projects and service registrations for compatible setup

* All features will now use a repository instead of using dbcontext directly. 

* Batch does not require a dbset name anymore, but does require a plural entity name for the FK if using one. Functionally, this is the same as before just with a new name for the prop: `ParentEntityPlural`

* Entity props now `virtual` by default with a `protected` constructor for mocking in unit tests. This is mostly for foreign entities (since we don't have EF to populate our foreign entities in unit tests), but in order to have our mocks accurately reflect all our props, we need to make them virtual. For example:

  ```c#
  private static Author GetMockAuthor()
  {
    	// my fake data base generated with AutoBogus under the hood
      var fakeRecipe = FakeRecipe.Generate();
      var forCreation = new FakeAuthorForCreationDto().Generate();
      forCreation.RecipeId = fakeRecipe.Id;
      
    	// FakeItEasy making entity assignment easy in lieu of EF
      var fakeAuthor = A.Fake<Author>(x => x.Wrapping(Author.Create(forCreation)));
      A.CallTo(() => fakeAuthor.Recipe)
          .Returns(fakeRecipe);
      return fakeAuthor;
  }
  ```

  > **💡 DOCS NOTE: this is default, but depending on your domain ops, you might have a domain method that does the action for you, in which case, you won't want these to be virtual. For example, Recipe might have `SetAuthor` or `AddIngredient` methods and you wouldn't want those FKs to be virtual**

* Add `FakeItEasy` and `FakeItEasyAnalyzer` to unit test project

* Colocate DTOs with entities by default. Can be moved to shared kernel when needed

* Default db provider set to postgres

* Removed `?`'s on strings in  `BaseEntity` 

* Update integration test consumer registration to non-obsolete model

* Testing cleanup

* Performance optimizations for integration and functional tests

  * Integration tests will only wipe the db using checkpoint at the begining of the entire fixture run, not after each test. This affects how assersions can and should be made
  * Unit tests have parallelization

* Removed sort integration tests

* Messages get a class for better typing as well as an interface.
  
* Add postman client to example scaffolding

* Add support for offline access and client secret requirement settings for client credentials clients

* Automatically add a `SuperAdmin` role claim to machines

* Updated services to use Newtonsoft to keep support for patchdocs. `System.Text.Json` is still available if patchdocs aren't needed for a project.

* Integration Test `TestBase` has global autobogus settings

* Permission and role validations are not case sensitive

* Added `client_role` to `UserPolicyHandler` role check to accomodate machines with a new scaffolded test.

  * Machine example:

    ```c#
    new Client
    {
      ClientId = "recipe_management.postman",
      ClientName = "RecipeManagement Postman",
      ClientSecrets = { new Secret("secret".Sha256()) },
    
      AllowedGrantTypes = GrantTypes.ClientCredentials,
      AllowOfflineAccess = true,
      RequireClientSecret = true,
      Claims = new List<ClientClaim>() { new(JwtClaimTypes.Role, "SuperAdmin") },
    
      RedirectUris = { "https://oauth.pstmn.io/v1/callback" },
      AllowedScopes = { "openid", "profile", "role", "recipe_management" }
    },
    ```


### Fixed

* Sql server connection strings to docker will trust the cert: `TrustServerCertificate=True;`



## [0.14.3] - 04/27/2022

### Fixed

- Nav link quote issue in BFF
- Lingering recipe route in BFF
- Added `useQuery` import to GET api in BFF

## [0.14.2] - 04/22/2022

### Fixed

- Bff conditional check

## [0.14.1] - 04/19/2022

### Fixed

- Missing dependencies (#73)
- LaunchSettings Casing (#72)

## [0.14.0] - 04/16/2022

### Added

* A `Dockerfile` and `.dockerignore` will be added to each bounded context automatically (except BFFs)

* A `docker-compose.yaml` will be added to your solution root by default for local development
  * Just run `docker-compose up --build` to spin up your databases (and RMQ if needed)

  * Then set an env and apply migrations. For a postgres example:
    * env

        *Powershell*
    
      ```powershell
      $Env:ASPNETCORE_ENVIRONMENT = "anything"
      ```
    
        *Bash*
    
      ```bash
      export ASPNETCORE_ENVIRONMENT=anything
      ```
    
    * `dotnet ef database update` or  `dotnet ef database update --connection "Host=localhost;Port=3125;Database=dev_recipemanagement;Username=postgres;Password=postgres"`
    
  * `SA` will always be default user for sqlserver so it can work properly

  * If no ports are given for api or db, they'll be auto assigned a free port on your machine

  * New `john` user to auth server (with no role)
  
  * Login hints for auth server
  
  * Minor helper override on fake generators
  
  * New `add:bff` command to add a bff to your solution
  
  * New `add:bffentity` command to add entities to your bff
  
  * Basic unit test scaffolding for create and update methods
  
  * Unit tests for updating rolepermissions


### Updated

- Fixed bug in `Add` feature that had a chained action after `ProjectTo`. They will now be filtered like so:

  ```c#
  var {entityNameLowercase}List = await _db.{entity.Plural}
  	.AsNoTracking()
   	.FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.{primaryKeyPropName} == {entityNameLowercase}.{primaryKeyPropName}, cancellationToken);
  
  return _mapper.Map<{readDto}>({entityNameLowercase}List);
  ```

- Removed `ProjectTo` from `GetRecord` feature in favor of direct mapper.

- Initial commit will use system git user and email as author. Courtesy of @sshquack

- `Id` on `BaseEntity` is sortable and filterable by default

- Minor logging updates for better json formatting and more information in prod

- GET record, PUT, and DELETE all have typed ids (e.g. `{id:guid}`) on their controllers

- `Development` environment uses a connection string to an actual database now, instead of an in memory db. This can easily be spun up with a `docker-compose` for local development

- Environment is now a singular object that will take in values for local environment variables for development and placed in launch settings and your docker compose. When deploying to other environments, you will use these same environment variables, but pass the appropriate value for that env.

  - Updated auth properties to be under an `AuthSettings` object like we do for broker settings
  - Removed `ConnectionString` from env. Docker connection will be added automatically in launch settings
  - Removed `EnvironmentName` as it will always be `Development`
  - Updated examples to handle new environment setup

- Removed unused mapper from delete feature

- Updated entity property definition with optional `ColumnType` attribute

- Fixed a bug that broke registration of the bus in development env

- Logger modifications

- Cleanup batch add feature

- Updated `Created` response for batch add

- Use entity plural directory for tests

- MassTransit bumped to v8

- Bumped Nuget packages to latest

- Updated checkpoint in test fixture for major version bump

### Removed

- Removed seeders

### Fixed

- Dbcontext for userpolicyhandler uses template (#70)
- Patch cancellation token added in feature
- Typo in role permission validation

## [0.13.1] - 02/25/2022

### Fixed

* `add:entity` command will now use the correct solution folder


## [0.13.0] - 01/27/2022

### Added

* Entities will now have private setters, a static `Create` method and an `Update` method to promote a DDD workflow
  * This includes base entity and the dbcontext setters for the auditable fields
* A huge permissions overhaul
  * `SuperAdmin` role is added by default and will automatically be given all permissions. 
  * A `User` role with also be created. This role will not have any permissions assigned to it by default and can be removed if you would like **as long as you add another role in addition to `SuperAdmin`**. This will allow for integration tests of the `UserPolicyHandler` to be fully scoped.
  * A permission will be added for each feature with a default name of `CanFEATURENAME`, but can be overridden if desired.
  * Role-permission mappings outside of this need tobe done manually 
  * You will be responsible for managing permissions outside of super admin
  * Policies are no longer part of the craftsman api, but a single policy will be added with each api boundary to swagger to dictate access to that boundary. It has a default of a snake case of `ProjectName`, but can be overridden
    * If using the built in auth server, this will be added for you. if not, make sure it mataches an appropriate scope in your auth server for this api boundary
  * Features now have a prop for `IsProtected` that, if true, will add an authorization attribute to your endpoint with the `PolicyName` and add tests that check for access
  * Integration tests for `UserPolicyHandler`
  * Unit tests for `RolePermission`
  * `Policies` prop removed from Feature
  * Added `SetUserRole` and `SetUserRoles` methods to integreation tests' `TestFixture` for easy role management in integration tests
  * Functional tests auth helper method now sets `role` instead of `scope`
  * Alice is a `SuperAdmin` and bob is a `User`
* Added a `register:producer` command with CLI prompt
* Added `UseSoftDelete` property to the `ApiTemplate` which is set to true. When adding an entity after the fact, Craftsman will automatically detect whether or not your project is using soft deletion by checking base entity for the appropriate property.
* Added a `SharedKernel` project at the root to capture DTOs, exceptions, and roles (if using auth)
* Added new `Complex` example for `new:example`


### Updated

* Updated FK and basic examples to have more features on the entities

* Updated tests to work with new private entity workflow

* CurrentUserServer has a method to get the User from the ClaimsPrincipal

* Swagger question removed from `add:feature` as it wasn't being used. Will be set to true.

* Removed unused `Unauthorized` and `Forbidden` MediatR filters 

* Test Fixture updated to better handle MassTransit

  * `_provider` will always be injected, even when not using MassTransit

  * The end of the fixture has been slightly updated to look like this:

    ```c#
            // MassTransit Harness Setup -- Do Not Delete Comment
    
            _provider = services.BuildServiceProvider();
            _scopeFactory = _provider.GetService<IServiceScopeFactory>();
    
            // MassTransit Start Setup -- Do Not Delete Comment
    ```

  * Added several helper methods to the test fixture

  * Updated the consumer test to use the helper methods

  * Producer doesn't take in placeholder props

  * Producer test generated

* Minor update to naming of producer in the bus example

* Default exchange type now `Fanout` for producer and consumer

* Added optional (but recommended) `DomainDirectory` prop to producers and consumers. This will move them from the `EventHandlers` directory and keep them directly with features for better colocation.

* Updated the 1:1 relationships to use proper scaffolding.

* Updated the FK example to show proper 1:1 relationship

* Entities with a Guid prop will no longer have a default of `Guid.NewGuid()`

* Updated default library from NewtonSoft.Json to System.text.Json (https://github.com/pdevito3/craftsman/issues/52)

* Took audit fields off of DTO

* Bumped LibGit2Sharp to preview for M1 compatibility

### Fixed

* Batch endpoint route updated to `batch` along with a functional testing route
* Batch add using update to controller
* `IHttpContextAccessor` fixted to a singleton in integration tests' `TestFixture`
* Can enter null for `add:feature` batch list options where needed
* Minor formatting fix for indentation in producers and consumers
* Removed extra exception using from  patch integration test
* Fixed docker image for integration tests running on macOS + M1 chip (https://github.com/pdevito3/craftsman/issues/53)

## [0.12.3] - 12/20/2021

### Update

* Updated Craftsman to .NET 6

## [0.12.2] - 12/03/2021

### Fixed

* Fixed bulk add file and variable names 

## [0.12.1] - 11/28/2021

### Updated

* Removed unused response class and endpoint reference
* Removed `Consumes` from Get list

## [0.12.0] - 11/28/2021

### Added

* Added `BaseEntity` that all entities will inherit from.

  * Contains a `Guid` of `Id` marked as the primary key
  * Contains `CreatedOn`, `CreatedBy`, `LastModifiedOn`, and `LastModifiedBy` properties

* Added a `CurrentUserService` to add a user to the `CreatedBy` and `LastModifiedBy` properties if a user is found. Built into db context

* Added built in features to the `add:feature` command

* New `AddListByFk` option for the `add:feature`  command and `Feature` property of an entity.

* New `craftsman example` or `craftsman new:example` command to create an example project with a prompted workflow to select. `Basic`, `WithAuth`, `AuthServer`, `WithBus`

* Added `.DS_Store` and `.env` to gitignore

* Added Consumer test

* Added `provider` to test fixture when adding a bus

* Added a mock `IPublishEndpoint` service to `TestFixture` when using MassTransit
  * update docs that mediatr handler tests aren't broken when pubilshing anymore

* New policies added to swagger on `add:entity` scaffolding

* New `add:authserver` command as well as an `AuthServer` option when creating a domain
  * No consent support (yet)

* Added helper `GetService` method to `TestFixture`

* Added Creation and Update Validators back to scaffolding. Easy enough to delete if you aren't using them

* Added a `NamingConvention` property to the db template

  * options are:

    ```
    Class
    SnakeCase
    LowerCase
    CamelCase
    UpperCase
    ```

### Updated

* Updated to .NET 6

* Updated nuget packages

  * Inlcudes a major release of Fluent Assertions that required updates to:
    * Functional test assertions (use `HttpStatusCode.XXX`)
    * Integration tests `TestBase` update for postgresto include `1.Seconds()`
    * Integration test updates to `await act.Should().ThrowAsync<` where appropriate
  
* Moved Policies to Feature

* There is no more primary key property. A Guid with a name `Id` will be inherited by all entities.

* Docker utilities for integration test refactored to use Fluent Docker wherever possible for better readability. Some enhancements were made as well (e.g. better container/volume naming, proper volume mounting).

* Removed `ErrorHandlerMiddleware` and replaced it with `ErrorHandlerFilterAttribute`

  * Updated built in Exceptions
  * Updated thrown errors and associated tests in the features

* Cleaned up test names

* Modified CORS util to take in env

* Added `Secret` back to Environment options

* Added local config utilities for testing environments

* Remove `UseInMemoryDb` app setting in favor of environment specific checks

* Remove `UseInMemoryBus` app setting in favor of environment specific checks

* Using statement shortening for reset function in test fixture

* Consolidated multiple environments startups to one startup file

* Update logging registration in `Program.cs` to no longer rely on `appsettings`

* Updated FK support to better API

* Moved env config from appsettings to environment variables

* Production env no longer added by default

* Features now include missing cancellation tokens as well as `AsNoTracking` properties

* Removed automatic fluent validation to allow more control in domain operations. For example:

  ```csharp
  var validator = new RecipeForCreationDtoValidator();
  validator.ValidateAndThrow(recipeForCreationDto);
  ```

  * this can be turned back on in `WebApiServiceExtension` by updating `.AddFluentValidation(cfg => { cfg.AutomaticValidationEnabled = false; });`


### Fixed

- Removed the broken patch validation from command
- No more `409` produced response annotation on POST
- Descending sort tests now actually test desc instead of mirroring asc
- Removed error handler comments in controller
- Empty controller no longer added when no features present (fixes #40)
- Messages project will be properly referenced when using a bus
- Swagger policies won't get duplicates

## [0.11.2] - TBD

### Fixed

- Better variable name on delete integ test

## [0.11.1] - 2021-08-23

### Fixed

- Removed extra parentheses on endpoint names

## [0.11.0] - 2021-08-22

### Added

- Added an `add:feature` command (also works with `new:feature`)
- Non-nullable guids will now have a default of `Guid.NewGuid()` unless otherwise specified
- Added XML docs to release in csproj
- Added a new `features` option to entities to allow for granular feature control. The accepted values are `AdHoc`, `GetRecord`, `GetList`, `DeleteRecord`, `UpdateRecord`, `PatchRecord`, `AddRecord`, and `CreateRecord` (same as `AddRecord` but available as an alias in case you can't remember!)

### Changed

- Instead of 3 BC projects (Core, Infra, Api) there will now be one. This helps with colocation and does force premature optimization for something like a model library and a heavily separated infra project.
  - Features dir changed to Domain with a features directory inside of it
  - Entities live on their respective entity folder in the domain with their feature
  - Removed the `webapi` suffix on the api project
  - Core directories moved to web api
  - `Contexts` dir changed to `Databases`
- Removed save successful checks on add and update commands
- Added better naming to PUT command variables
- Guid PKs will no longer be added to the creation DTO
- POST commands will no longer have a conflict check since you can't add a PK anymore
- A conflict integration test will not be added anymore
- `NoKeyGenerated` commands are no longer in dbcontext
- Controller url all lowercase
- Swagger comments on by default
- 409 no longer shown on swagegr comments for POST
- No more save successful check on delete command
- Seeders have a sub `DummyData` directory
- Sieve service registration moved to webapi service class
- `SolutionName` to `ProjectName`

### Fixed

- Seeder indentation in startup fixed
- PUT commands will no longer throw 500 when entity is not modified (#31)
- Route indentation fixed
- Removed annoying comments from features
- Fixed test name for basic gets in functional tests
- Seeders in startup will newline when there are multiple entities
- Unicode now onlyenforced on windows (for better emoji support)
- Fixed `isRequired` property
- Command prop for `Update` command has proper casing

### Removed

- Removed the `add:property` cli command

## [0.10.0] - 2021-05-31

### Added

- Added [SpectreConsole](https://spectreconsole.net/) for a better CLI experience
- Added `add:bus` command
- Added `add:consumer` and `add:producer` commands for direct, topic, and fanout messages
- Added Bus, Producers, and Consumers props to BC template
- Added Messages to the domain template
- Added `add:message` command
- Added conflict test for add command when using a guid
- Added tests for command and query exceptions
  - Didn't do them in the controller as that is not the dependency. Can test the exceptions causing the correct httpstatus code in the exception separately

### Changed

* Changed the `new:domain` output to a single solution with directories for each bounded context for easier management
* Changed the seeder regions in `StartupDevelopment.cs` to comments
* Changed the Logger settings in `Program.cs`
* Updated `add:entity` and `add:prop` to now be called from the BC directory
* Updated `ProducesResponseType` in controllers to generic `Response` type where applicable
* Updated App Registrations to separate files
* Updated Service Registrations to separate files
* Updated entity name and entity prop names first letter to always be capitalized
* Better namespacing for features in controllers using static classes for features
* Updated functional test to pass without conflict
* Updated nuget packages
* Updated `Program.cs` to async
* Changed migrations to happen after all bounded contexts are added
* Removed the custom fluent validation boilderplate from the add, update, patch, and ad hoc commands

### Removed

- Removed verbosity option from commands due to simplified spectre console
- Removed legacy comment for include statement marker
- Removed BC readme and updated sln readme

### Fixed

- Fixed double error messages
- Fixed incorrect help message for `new:domain` command (#24)
- Fixed help text on `list` command
- Fixed controllers to inherit from `ControllerBase` instead of `Controller`. fixes (#26)
- Fixed extra space in the class in the dto classes when not abstract and trailing new line
- If using a guid for a PK, it will be added to the creation dto (not manipulation or update) -- fixes #28
  - Guid PKs will have a default value of `Guid.NewGuid()` in their creation dto
- PK already exists guard will be added for GUIDs and will be performed when adding a new entity and throw a 409 conflict via a new conflict exception if a record already exists with that guid. -- fixes #29
- Fixed issue where POST would throw 500 when primary key != EntityNameId (e.g. PK of ReportId would break for an entity of ReportRequest) - fixes #30
- Fixed default value for strings on entities to use quotes
- Fixed missing exception handling on `add:bc` command

## [0.9.3] - 2021-04-10

### Fixed

- Fixed autogen identity

## [0.9.2] - 2021-04-10

### Fixed

- Lingering dbcontext and dbname mix up

## [0.9.1] - 2021-04-10

### Fixed

- Db context will now be used instead of name in api scaffolding
- Test utilities in functional tests will now be added when not using auth

## [0.9.0] - 2021-04-10

### Added

- Added a new vertical slice architecture
  - Projects have been consolidated and will now have a prefix of the solution name before each project type. For example, the api project with a solution name of `ordering` is `ordering.webapi`
- Added a `new:domain` command to create a ddd based domain with various bounded contexts inside of it. this is recommended for long term maintainability
- Added the `add:bc` command which will add a new bounded context to your ddd project
- Testing completely rebuilt from the ground up. Now has unit, integration, and functional tests. Integration and Functional Tests can spin up their own docker db on their own to run against a real database.
- Moved 'addGit' property from the api template to the domain template
- Added a `version` or `-v` command to get the craftsman version
- Added an initial db migration to run automatically on project creation
- Added verbosity option to new domain and add bc
- Added a version checker to make sure you are alerted if out of date
- Added an `add:prop` alias
- Added explicit add entity template with auth policies available to add
- Added a production app settings by default

### Changed

- Changed the startup marker for dynamic services to a comment instead of a region
- Readme will now be generated in the domain directory
- Updated environment to have production as a reserved word instead of startup to be consistent with dotnet process
  - Will use startup and appsettings.production
  - Normal appsettings will be empty, but have all the config keys required to make migrations and builds possible
- Updated the default Cors policy name
- Consolidated launchsettings to have the same setup for all environments as it is just a setting for the IDE and not used for the release package

### Removed

- Removed `micro` command to consolidate and reduce complexity. if you still want to build a microservice, you can build a domain and deploy each bounded context as a microservice
  - Gateways were removed and may be added back with better integration in a future release
- Removed the `new:api` command to focus on the DDD driven style
- Removed `ClientSecret` to promote code+PKCE flow

### Fixed

- Existing auth policies will now be skipped for registration when adding a new entity
- Fixed documented response codes for delete, put, and patch from 201 > 204
- Foreign keys will no longer be automatically included in features or DTOs for better performance (#2)

## [0.8.2] - 2021-02-25

### Fixed

- Fixed issue where xml comments would throw an error on non windows machines (#16)

## [0.8.1] - 2021-02-22

### Fixed

- fixed bug when creating an api with auth settings

## [0.8.0] - 2021-02-22

### Added

- New `add:micro` command that scaffolds a new microservice template as well as an ocelot gateway
- New `port ` property on the `new:api` template to let you customize and api or microservice port on localhost
- Added `https` default on local
- Added additional startup middleware
  - `UseHsts` for non dev environments
  - `UseHttpsRedirection` with notes on even more secure options
- New `AuthorizationSettings` object and authorization based properties on the environments for the `new:api` and `new:micro` commands
- Added new `GetEntity` and `DeleteEntity` integration tests with and without auth
- Added 401/403 response types to swagger comments when using auth
- Added auth to swagger setup
  - note that secret is currently stored in appsettings
- Auth added to integration tests when required

### Fixed

- The `CurrentStartIndex` calculation in the `PagedList` class was broken and now has a new calculation.
- Added null conditional operator (`?.`) to certain tests before `.Data` to make them fail more gracefully
  - Get{entity.Plural}_ReturnsSuccessCodeAndResourceWithAccurateFields()
  - Put{entity.Name}ReturnsBodyAndFieldsWereSuccessfullyUpdated
- Cleaned up `WebApplicationFactory` to remove deprecated services.
- Removed `[Collection(""Sequential"")]` from repo tests

### Clean up

- Internal tests now passing
- refactored out template drilling
- removed old auth debt from earlier alpha

## [0.7.0] - 2021-01-12

### Added

- Removed the dependency on the foundation api template!

### Fixed

- Fixed `UseEnvironment` in WebAppFactory to use `Development`
- Fixed integration tests to use the new `Response` wrapper
- Updated pagination tests to have proper keys due to default sort order possibly breaking these tests

## [0.6.1] - 2021-01-06

### Fixed

- XML comment info is now properly added to `WebApi.csproj` and the Swagger config
- Extra line will no longer be added when no swagger contact url is provided
- Repository now sets default sort order for proper sql compatibility in lists (issue #9)

## [0.6.0] - 2020-12-22

### Added

- Added table name and schema properties to entity
- Added column name attribute to entity properties
- Added Serilog by default in all new projects. This includes Console and Seq logging by default in `Development`. For non-Development environments, you'll need to add whatever logging you're interested in to their respective app-settings projects. There are just too many options to create a whole API on top of Serilog.
- Updated swagger implementation from nswag
- Added Consumes and Produces headers to the controller endpoints

- Added an option to manage additional swagger settings to your API endpoints. This will be turned off by default for now as dealing the with xml docs path is potentially burdensome, but will add a lot of valuable details for users consuming your API. If you are looking to add additional XML details, this is highly recommended. 
- Added a custom Response Wrapper to the GET and POST endpoints

### Fixed

- Fixed launch settings to have null environment  variable for Startup (Production). If you'd like to change this, be sure to update the appsetting lookup in `Program.cs`
- Fixed POST endpoint that was lacking a `[FromBody]` marker
- `BasePaginationParameters` will now have `MaxPageSizee` and `DefaultPageSize` set as `internal` properties so they don't show up in swagger. These can be overridden in the distinct entity classes like so: `internal override int MaxPageSize { get; } = 30;`
- Fixed controllers to be able to handle a name and plural with the same value (e.g. Buffalo)

## [0.5.0] - 2020-12-14

### Added

- Added `add:entities` alias for the `add:entity` command
- Can now add Guid or other non-integer primary key

### Fixed

- Fixed bug where postgres library was getting added every time

## [0.4.2] - 2020-12-10

### Fixed

- Seeder was not getting added to `StartupDevelopment` when using `add:entity` command

## [0.4.1] - 2020-12-10

### Fixed

- Async method in controller POST wasn't awaited

## [0.4.0] - 2020-12-06

### Added

- Default `Startup.cs` class can now be configured using the reserved `Startup` keyword

### Fixed

- Fixed `craftsman add:property -h` help text
- The appsettings connection string will now escape backslash
- Foreign key using statement will now be dynamic on DTOs

## [0.3.1] - 2020-12-02

### Added

- Added `new:webapi` alias that acts the same as `new:api`

### Fixed

- Fixed `craftsman add:property -h` to point to the correct help page

## [0.3.0] - 2020-11-14

### Added

- Updated the API to run on NET 5.0
- Pagination metadata enhancements on PagedList that is returned in  GET list endpoint will now include more metadata for the current page  including the current size as well as the start and end indices. I also  removed the Next and Previous Page URI links to reduce complexity.
- Updated all controller calls to be asynchronous, including the get list
- Saves updated to be asynchronous
- One major capability I want to add into this is a good basis for auth  generation. I've started to build this out, but it could (and very  likely will) change drastically. With that said, I left it in as an  alpha feature in case anyone is interested in trying it.

### Fixed

- Add Entity bug in repository fixed
- Fixed some builder options when not using auth
