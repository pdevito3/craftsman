# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Added table name and schema properties to entity

- Added Serilog by default in all new projects. This includes Console and Seq logging by default in `Development`. For non-Development environments, you'll need to add whatever logging you're interested in to their respective app-settings projects. There are just too many options to create a whole API on top of Serilog.

  - If you have an existing project and want to add this to your project, you'll need do this manually :

    - Add the following packages to webapi:

        <PackageReference Include="Serilog.AspNetCore" Version="3.4.0" />
        <PackageReference Include="Serilog.Enrichers.AspNetCore" Version="1.0.0" />
        <PackageReference Include="Serilog.Enrichers.Context" Version="4.2.0" />
        <PackageReference Include="Serilog.Enrichers.Environment" Version="2.1.3" />
        <PackageReference Include="Serilog.Enrichers.Process" Version="2.0.1" />
        <PackageReference Include="Serilog.Enrichers.Thread" Version="3.1.0" />
        <PackageReference Include="Serilog.Settings.Configuration" Version="3.1.0" />
        <PackageReference Include="Serilog.Sinks.Console" Version="3.1.1" />
        <PackageReference Include="Serilog.Sinks.Seq" Version="4.0.0" />

    - Update your `Program` to the following:

    ```csharp
    namespace WebApi
    {
        using Autofac.Extensions.DependencyInjection;
        using Microsoft.AspNetCore.Hosting;
        using Microsoft.Extensions.Configuration;
        using Microsoft.Extensions.Hosting;
        using Serilog;
        using System;
        using System.IO;
        using System.Reflection;
    
        public class Program
        {
            public static void Main(string[] args)
            {
                var myEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                var appSettings = myEnv == null ? $"appsettings.json" : $"appsettings.{myEnv}.json";
    
                //Read Configuration from appSettings
                var config = new ConfigurationBuilder()
                    .AddJsonFile(appSettings)
                    .Build();
    
                //Initialize Logger
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(config)
                    .CreateLogger();
    
                try
                {
                    Log.Information("Starting application");
                    CreateHostBuilder(args)
                        .Build()
                        .Run();
                }
                catch (Exception e)
                {
                    Log.Error(e, "The application failed to start correctly");
                    throw;
                }
                finally
                {
                    Log.Information("Shutting down application");
                    Log.CloseAndFlush();
                }
            }
    
            public static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                    .UseSerilog()
                    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup(typeof(Startup).GetTypeInfo().Assembly.FullName)
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseKestrel();
                    });
        }
    }
    ```

    

    - Update your `appsettings.Development` to:

    ```json
    {
      "AllowedHosts": "*",
      "UseInMemoryDatabase": true,
      "Serilog": {
        "Using": [],
        "MinimumLevel": {
          "Default": "Information",
          "Override": {
            "Microsoft": "Warning",
            "System": "Warning"
          }
        },
        "Enrich": [ "FromLogContext", "WithMachineName", "WithProcessId", "WithThreadId" ],
        "WriteTo": [
          { "Name": "Console" },
          {
            "Name": "Seq",
            "Args": {
              "serverUrl": "http://localhost:5341"
            }
          }
        ]
      },
    }
    ```

- Updated swagger implementation from nswag

  - Add the below packages to your webapi project and remove the `Nswag` package:

    ```
    <PackageReference Include="Microsoft.AspNetCore.Authentication.OpenIdConnect" Version="5.0.1" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="5.6.3" />
    ```

  - In Webapi `ServiceExtensions` replace `using NSwag` with `using Microsoft.OpenApi.Models;` and update the swagger region to something like the below with the appropriate info for your project:

    ```csharp
            #region Swagger Region - Do Not Delete
                public static void AddSwaggerExtension(this IServiceCollection services)
                {
                    services.AddSwaggerGen(config =>
                    {
                        config.SwaggerDoc(
                            "v1", 
                            new OpenApiInfo
                            {
                                Version = "v1",
                                Title = "",
                                Description = "",
                                Contact = new OpenApiContact
                                {
                                    Name = "",
                                    Email = "",
                                    Url = new Uri(""),
                                },
                            });
                    });
                }
            #endregion
    ```

  - In Webapi `AppExtensions`, update the swagger region to the below:

    ```csharp
            #region Swagger Region - Do Not Delete
            public static void UseSwaggerExtension(this IApplicationBuilder app)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "");
                });
            }
            #endregion
    ```

- Added Consumes and Produces headers to the controller endpoints
- Added an option to manage additional swagger settings to your API endpoints. This will be turned off by default for now as dealing the with xml docs path is potentially burdensome, but will add a lot of valuable details for users consuming your API. If you are looking to add additional XML details, this is highly recommended. 
- Added a custom Response Wrapper to the GET and POST endpoints

### Fixed

- Fixed launch settings to have null environment  variable for Startup (Production). If you'd like to change this, be sure to update the appsetting lookup in `Program.cs`
- Fixed POST endpoint that was lacking a `[FromBody]` marker
- `BasePaginationParameters` will now have `MaxPageSizee` and `DefaultPageSize` set as `internal` properties so they don't show up in swagger. These can be overridden in the distinct entity classes like so: `internal override int MaxPageSize { get; } = 30;`

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
