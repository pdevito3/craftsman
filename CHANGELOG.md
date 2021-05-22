# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

- Added SpectreConsole for a better CLI experience
- Fixed double error messages
- Fixed incorrect help message for `new:domain` command (#24)
- Fixed help text on `list` command
- Removed verbosity effectiveness from `new:domain`
  - also do this in other commands?
- Changed the seeder regions in `StartupDevelopment.cs` to comments
- Cleaned up the Logger settings in `Program.cs`
- Updated `Program.cs` to async
- Changed the `new:domain` output to a single solution with directories for each bounded context for easier management
- Updated `add:entity` and `add:prop` to now be called from the BC directory
  - UPDATE WRAPT DOCS FOR THIS
- Fixed controllers to inherit from `ControllerBase` instead of `Controller`. fixes (#26)
- No extra space in the class in the dto classes when not abstract and trailing new line
- If using a guid for a PK, it will be added to the creation dto (not manipulation or update) -- fixes #28
- Guid PKs will have a default value of `Guid.NewGuid()` in their creation dto -- fixes #28
- PK already exists guard will be performed when adding a new entity and throw a 409 conflict via a new conflict exception if a record already exists with that guid. -- fixes #29
  - **Add an integ test for this**
- Updated `ProducesResponseType` in controllers to generic `Response` type where applicable
- Removed legacy comment for include statement marker
- Fixed issue where POST would throw 500 when primary key != EntityNameId (e.g. PK of ReportId would break for an entity of ReportRequest) - fixes #30
- Moved App Registrations to separate files
- Moved Service Registrations to separate files
- Fixed default value for strings on entities to use quotes
- Fixed missing exception handling on `add:bc` command
- Added `add:bus` command
- Added Bus, Messages, Producers, and Consumers props to BC template
- Added `add:message` command
- Updated entity name and entity prop names first letter to always be capitalized
- Added `add:consumer` and `add:producer` commands
- Removed BC readme and updated sln readme
- Better namespacing for features in controllers using static classes for features
- Updated functional test to pass without conflict

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