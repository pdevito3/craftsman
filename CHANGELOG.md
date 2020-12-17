# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Added table name and schema properties to entity

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
