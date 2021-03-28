# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

- consolidated projects
- projects will now have a prefix of the solution name before each project type. for example, the api project with a solution name of `ordering` is `ordering.webapi`
- added a `new:domain` command to create a ddd based domain with various bounded contexts inside of it. this is recommended for long term maintainability
- removed `micro` command to consolidate and reduce complexity. if you still want to build a microservice, you can build a domain and deploy each bounded context as a microservice
  - Gateways were removed though and will be added back in with better integration in a future release
- removed the `new:api` command to focus on the DDD command
- added the `add:bc` command which will add a new bounded context to your ddd project
- Moved 'addGit' property from the api template to the domain template
- readme will now be generated in the domain directory
- added a `version` or `-v` command to get the craftsman version
- an initial db migration will automatically be ran for you on project creation
  - NEED TO UPDATE MIGRATION DOCS
  - need to add dotnet-ef and docker as optional prereq if they want migrations (and working integ tests)
- testing completely revamped
  - **Unit tests** are meant to confirm that individual operations are working as expected (e.g. PagedList calculations)
  - **Integration tests** are meant to check that different areas are working together as expected (e.g. our features folder). These tests rebuild from the ground up using NUnit. It will now spin up a real db in docker and run each of your feature tests here
  - **Functional tests** are meant to check an entire slice of functionality with all the code running together. These are generally more involved to write and maintain, but with this project setup, our controllers are essentially just exposed routes to our feature queries and commands. This means that we have already done the meat of our testing in our integration tests, so these tests will just confirm that we are getting the expected responses from our routes.
- verbosity option added to new domain and add bc
- removed `ClientSecret` to promote code+PKCE flow
  - update auth explanation and environment object docs
  - make sure example files don't include it
- Added a version checker to make sure you are alerted if out of date
- Known postgres integration test issue with datetime precision
- added an `add:prop` alias
- proper add entity template added
  - entities is only required prop. db and solutionname are calculated
  - auth settings available for policies to add
- fix: existing auth policies will now be skipped for registration when adding a new entity
- Added a production app settings by default
- fixed environment to have production as a reserved word instead of startup to be consistent with dotnet process
  - will use startup and appsettings.production
  - normal appsettings will be emtpy, but have all the config keys required to make migrations and builds possible
- consolidated launchsettings tohave the same setup for all environments as it is just a setting for the IDE and not used for the release package

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
