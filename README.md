# Craftsman

## Description

This is a custom .NET Core tool that is used to accelerate your API development workflow by quickly scaffolding out new files and projects with simple CLI commands and configuration files.

## Prerequisites

- VS2019 .Net Core Feature Set or .Net Core 3.1 SDK https://dotnet.microsoft.com/download/dotnet-core/3.1

## About Craftsman

Craftsman provides new commands to your CLI that will enable you to quickly scaffold out an API and easily make modifications to it over time. In some instances, this will require you to scaffold out your basic API configuration in a `yaml` or `json` file for the `craftsman` command  to consume. Don't feel pressured to get this exactly correct from the get go. **This file is NOT meant to be a concrete implementation of your API**, just a starting point that you can build on over time. 

## Getting Started

1. For now, you'll need to install the [Foundation API Template](https://github.com/pdevito3/foundation.api):

   ```shell
   dotnet new -i foundation.api
   ```

   > Note that on the off chance that you have another dotnet template on your machine with a `foundation` short name, you will need to uninstall that template before using a `crafstman new:api` command

   

2. Then, install the `Craftsman` tool globally

   ```shell
   dotnet tool install -g craftsman
   ```

   You can then run `dotnet tool update -g craftsman` to update the package in the future.

3. Run any of the below for a list of available commands

   ```shell
   craftsman 
   craftsman list
   craftsman -h
   craftsman --help
   ```

4. That's it! Now we can [build our API](#new-api-command).

## Commands

### New API Command

To create our initial project, we are going to use the `new:api` command. You can get help for this command by running:

```shell
craftsman new:api -h
```

#### New API Config File

To get things started, you're going to need to scaffold out your initial API configuration in a `yaml` or `json` file. Don't feel pressured to get this exactly correct from the get go. **This file is NOT meant to be a concrete implementation of your API**, just a starting point that you can build on over time. 

So what's this file look like? For a full list of the configuration option details, you can go [here](#api-configuration-file-options), but let's go over a basic `yaml` example:

First, you need to start with a name for your API:

```yaml
SolutionName: VetClinic.Api
```

Next, let's layout the database context:

```yaml
SolutionName: VetClinic.Api
DbContext:
  ContextName: VetClinicContext
  DatabaseName: VetClinic
  Provider: SqlServer
```

Then, we'll add an entity:

```yaml
SolutionName: VetClinic.Api
DbContext:
  ContextName: VetClinicContext
  DatabaseName: VetClinic
  Provider: SqlServer
Entities:
  - Name: Pet
    Properties:
    - Name: PetId
      IsPrimaryKey: true
      Type: int
      CanFilter: false
    - Name: Name
      Type: string
      CanFilter: false
    - Name: Type
      Type: string
      CanFilter: false
      CanSort: true
```

Note that this is a list, so we can add more than one entity if we want:

```yaml
SolutionName: VetClinic.Api
DbContext:
  ContextName: VetClinicContext
  DatabaseName: VetClinic
  Provider: SqlServer
Entities:
  - Name: Pet
    Properties:
    - Name: PetId
      IsPrimaryKey: true
      Type: int
      CanFilter: false
    - Name: Name
      Type: string
      CanFilter: false
    - Name: Type
      Type: string
      CanFilter: false
      CanSort: true
  - Name: City
    Plural: Cities
    Properties:
    - Name: CityId
      CanManipulate: false
      Type: int
      CanFilter: true
    - Name: Name
      Type: string
      CanFilter: false  
```

Now, let's [make our API](#using-the-command)!

#### Using the Command

1. To actually create our API, you'll want to `cd` into whatever directory you want to add your project to:

```shell
cd C:\MyFull\RepoPath\Here
```

2. Then, let's make sure that our `Foundation.Api` template is up to date:

```shell
dotnet new foundation.api --update-apply
```

3. Then we just need to add our `yaml` or `json` path to our `craftsman new:api` command:

```shell
craftsman new:api C:\Users\Paul\Documents\ApiConfigs\VetClinic.yaml
```

4. Once you've run this command, your API should be ready to go!

#### API Structure

Instead of making you learn some custom directory structure, this template was built using the well known clean architecture format. If you are not familiar with it, the here is a brief overview of the overall concept.

##### Core

The core layer is split into two projects, the `Application` and `Domain` layers. 

The `Domain` project is pretty simple and will capture all of the entities and items directly related to that. This layer should never depend on any other project.

The `Application` project is meant to abstract out our specific business rules for our application. It is dependent on the `Domain` layer, but has no dependencies on any other layer or project. This layer defines our interfaces, DTOs, Enums, Exceptions, Mapping Profiles, Validators, Specifications, and Wrappers that can be used by our external layers.

##### Infrastructure

Our infrastructure layer is used for all of our external communication requirements (e.g. database communication). For more control, this layer is split into a `Persistence`  project as well as a `Shared` project. Additional layers like `Auth`, `Web Api Clients`, `File System Accessors`, `Logging Adapters`, and `Email/SMS Sending` can also be added here.

The `Persistence`  project will capture our application specific database configuration. The `Shared` project will capture any external service requirements that we may need across our infrastructure layer (e.g. DateTimeService).

##### API

Finally, we have our API layer. This is where our `WebApi` project will live to provide us access to our API endpoints. This layer depends on both the `Core` and `Infrastructure` layers,  however, the dependency on `Infrastructure` is only to support dependency  injection. Therefore only `Startup` classes should reference `Infrastructure`.

### Add Entity Command

At some point, you're going to need to add a new entity to your project. We can do this using the `add:entity` command. You can get help for this command by running:

```shell
craftsman add:entity -h
```

#### Add Entity Config File

First, you'll want to create a `yaml` or `json` file that describes the new entity. This is pretty much the same as the file we made for our `new:api` command, but now, the only required section is the `Entities` section. Just create a list of one or more entities, and you're good to go. For example:

```yaml
Entities:
  - Name: Supplier
    Properties:
      - Name: SupplierId
        IsPrimaryKey: true
        Type: int
        CanFilter: true
        CanSort: true
      - Name: Name
        Type: string
        CanFilter: true
        CanSort: true
      - Name: EmployeeCount
        Type: int?
        CanFilter: true
        CanSort: true
      - Name: CreationDate
        Type: datetime?
        CanFilter: true
        CanSort: true
      - Name: SupplierType
        Type: int?
        CanFilter: true
        CanSort: true
```

#### Using the Command

To actually add the entity, you'll want to `cd` into whatever directory your API solution is located at.

```shell
cd C:\MyFull\RepoPath\Here
```

Then, we just need to add our `yaml` or `json` path to our `craftsman add:entity` command:

```shell
craftsman add:entity C:\Users\Paul\Documents\ApiConfigs\supplier-entity.yaml
```



### Add Entity Property Command

At some point, you're going to need to add a new property to your entities. This command will allow you to easily add that property to not just your entity, but also it's associated DTOs. You can get help for this command by running:

```shell
craftsman add:property -h
```

To actually create an entity, you will do something like this:

```shell
craftsman add:property -entity Vet -name VetName -type string -filter false -sort true
```
Also the same as:

```shell
craftsman add:property -e Vet -n VetName -t string -f false -s true
```

Note that  arguments that are not required do not need to have values, so something like this is also comparable to the above:

```shell
craftsman add:property -e Vet -n VetName -t string
```

#### Arguments

| Argument        | Required | Type    | Description                                                  |
| --------------- | -------- | ------- | ------------------------------------------------------------ |
| -e, -entity     | Yes      | Text    | Name of the entity to add the property. Must match the name of the entity file (e.g. `Vet.cs` should be `Vet`) |
| -n, -name       | Yes      | Text    | Name of the property to add                                  |
| -t, -type       | Yes      | Text    | Data type of the property to add                             |
| -f, -filter     | No       | Boolean | Determines if the property is filterable                     |
| -s, --sort      | No       | Boolean | Determines if the property is sortable                       |
| -k, -foreignkey | No       | Text    | When adding an object linked by a foreign key, use this field to enter the name of the property that acts as the foreign key |

#### Foreign Keys

Similar to when we're adding an entity with a [foreign key in a file](#example-with-foreign-key), we can add a foreign key property ad hoc using the `add:property` command. Let's say that we had a 'Sale' entity that has a `ProductId` property that acts as a foreign key to another 'Product' entity. If we wanted to return the product in our responses, we could add an additional `Product` property of type `Product` that links to the primary key of `ProductId` in our `Product` entity:

```shell
crafstman add:property -e Sale -n Product -t Product -k ProductId
```



## API Configuration File Options

### SolutionName

| Name          | Required | Description                                                  | Default |
| ------------- | -------- | ------------------------------------------------------------ | ------- |
| Solution Name | Yes      | A single key value pair that designates the name you want to use for your API. | *None*  |

#### Example

```yaml
SolutionName: VetClinic.Api
```



### DbContext

An dictionary that describes the basics of your database with the following key. For now, only one application DbContext is supported, with plans to expand this in a later release.

| Name          | Required | Description                                                  | Default   |
| ------------- | -------- | ------------------------------------------------------------ | --------- |
| DbContextName | Yes      | A single key value pair that designates the name you want to use for your API. | *None*    |
| DatabaseName  | Yes      | The name of your database.                                   | *None*    |
| Provider      | No       | The database provider for your DbContext.                    | SqlServer |

#### Example

```yaml
DbContext:
  ContextName: TestDbContext
  DatabaseName: Test
  Provider: SqlServer
```



### Entities

An list of database entities in your project. These entities are added as dbsets to your database context, so they translate 1:1 to tables in your database. 

| Name       | Required | Description                                                  | Default                          |
| ---------- | -------- | ------------------------------------------------------------ | -------------------------------- |
| Name       | Yes      | The name of the entity                                       | *None*                           |
| Plural     | No       | The plural of the entity name, if needed (e.g. `Cities` would prevent `Citys`) | Entity Name appended with an `s` |
| Properties | No       | A list of properties assigned to your entity described [here](#entity-properties) | *None*                           |

#### Entity Properties

An list of properties assigned to an entity.

| Name               | Required | Description                                                  | Default                          |
| ------------------ | -------- | ------------------------------------------------------------ | -------------------------------- |
| Name               | Yes      | The name of the property                                     | *None*                           |
| Type               | Yes      | The data type for the property. These are *not* case sensitive and they can be set to nullable with a trailing `?`. | *None*                           |
| IsPrimaryKey       | Yes      | When true, the property will be set as the primary key for the entity. For now, only one primary key is supported, with plans to add compound key support down the road. | false                            |
| CanFilter          | No       | Will set the property to be filterable in the API endpoint when set to true. | false                            |
| CanSort            | No       | Will set the property to be filterable in the API endpoint when set to true. | false                            |
| IsRequired         | No       | When true, the property will be set as required in the database. | false<br/>*true for primary key* |
| CanManipulate      | No       | When set to false, you will not be able to update this property when calling the associated endpoint. When set to `false`, the property will be able to be established when using the POST endpoint, but will not be able to be updated after that. This is managed by the DTOs if you want to manually adjust this. | true<br/>*false for primary key* |
| ForeignKeyPropName | No       | When adding an object linked by a foreign key, use `ForeignKeyPropName` to enter the name of the property that acts as the foreign key |                                  |

#### Example

```yaml
Entities:
  - Name: Supplier
    Properties:
      - Name: SupplierId
        IsPrimaryKey: true
        Type: int
        CanFilter: true
        CanSort: true
      - Name: Name
        Type: string
        CanFilter: true
        CanSort: true
      - Name: EmployeeCount
        Type: int?
        CanFilter: true
        CanSort: true
      - Name: CreationDate
        Type: datetime?
        CanFilter: true
        CanSort: true
      - Name: SupplierType
        Type: int?
        CanFilter: true
        CanSort: true
```

#### Example With Foreign Key

So let's say that we had a 'Sale' entity that has a `ProductId` property that acts as a foreign key to another 'Product' entity:

```csharp
- Name: Sale
  Properties:
  - Name: SaleId
    IsPrimaryKey: true
    Type: int
    CanFilter: true
  - Name: ProductId
    Type: int
    CanFilter: true
  - Name: SaleDate
    Type: DateTimeOffset?
    CanFilter: true
  - Name: SaleAmount
    Type: int
    CanFilter: true
```

If we wanted to return the product in our responses, we could add an additional `Product` property of type `Product` that links to the primary key of `ProductId` in our `Product` entity:

```yaml
- Name: Sale
  Properties:
  - Name: SaleId
    IsPrimaryKey: true
    Type: int
    CanFilter: true
  - Name: ProductId
    Type: int
    CanFilter: true
  - Name: SaleDate
    Type: DateTimeOffset?
    CanFilter: true
  - Name: SaleAmount
    Type: int
    CanFilter: true
  - Name: Product
    Type: Product
    ForeignKeyPropName: ProductId
```

### Swagger/OpenAPI (Optional)

If desired, you can add a basic swagger configuration using the `SwaggerConfig` option. This will scaffold out a barebones swagger pager using [NSwag](https://github.com/RicoSuter/NSwag), which you can update to use any additional features you wish. To get to your swagger page, you can go to `/swagger`. To get to your swagger json, you can go to `/swagger/v1/swagger.json`.

| Name        | Required | Description                          | Default |
| ----------- | -------- | ------------------------------------ | ------- |
| Title       | Yes      | The title of the swagger document    | *None*  |
| Description | No       | The description for your swagger doc | *None*  |
| ApiContact  | No       | The API contact                      | *None*  |

#### ApiContact

| Name  | Required | Description                 | Default |
| ----- | -------- | --------------------------- | ------- |
| Name  | No       | The name of the contact     | *None*  |
| Email | No       | The email of the contact    | *None*  |
| Url   | No       | The website for the contact | *None*  |



#### Example

```yaml
SwaggerConfig:
  Title: MySwaggerDoc
  Description: This is my swagger doc
  ApiContact: 
    Name: Paul
    Email: paul@test.com
    Url: https://www.mywebsite.com
```



### Multiple Environments (Optional)

The template is configured with one environment by default, with the ability to add as many additional environments as you'd like. By default, the project will run `Development` . This is configured to run locally and use an in-memory database by default and should not be connected to a live database.  This ensures that all users will be able to run the solution without  needing to go through additional database set up.

Additional environments are added using an `Environments` list in your initial build file. This will use a multiple startup approach as described [here](https://aspnetcore.readthedocs.io/en/stable/fundamentals/environments.html). This means that there will be a `Startup{EnvironmentName}.cs` class, and `appsettings.{environment}.json` file, and a `launchsettings.json` profile added for each environment.

#### Environment Properties

| Name             | Required | Description                                                  | Default |
| ---------------- | -------- | ------------------------------------------------------------ | ------- |
| EnvironmentName  | Yes      | The name of the environment that will set `ASPNETCORE_ENVIRONMENT`, the suffix of your `Startup.cs` etc. | *None*  |
| ConnectionString | Yes      | The connection string for the database. The connection string will just be added in between quotes and treated like a string, so will need to be valid in that format | *None*  |
| ProfileName      | Yes      | The name of the profile that is added into your `launchsettings.json` to run this environment | *None*  |

> IMPORTANT
>
> Note that the connection strings will currently be added to `appsettings.{environment}.json`, so they will be committed to source control. You should NOT be adding anything with credentials in here. In the future, I'll be adding the option for using secret manager and integration with external secret managers in AWS, Azure, etc.

#### Example

```yaml
Environments:
  - EnvironmentName: Production
    ConnectionString: "Data Source=prodserver;Initial Catalog=MyDbName;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;"
    ProfileName: Prod
  - EnvironmentName: Qa
    ConnectionString: "Data Source=qaserver;Initial Catalog=MyDbName;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;"
    ProfileName: Qa
  - EnvironmentName: Staging
    ConnectionString: "Data Source=stagingserver;Initial Catalog=MyDbName;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;"
    ProfileName: Staging
  - EnvironmentName: Local
    ConnectionString: "Data Source=localhost\\SqlExpress;Initial Catalog=MyDbName;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;"
    ProfileName: Local
```



## Using Database Migrations

Currently, migrations are a pain to deal with when you have multiple environment files like this as it is not a smooth process to differentiate between the right `appsettings` file. If you'd like to use them you will need to set `ASPNETCORE_ENVIRONMENT` before running `dotnet ef` to let the host builder locate the appropriate settings file. Here's some examples for [Windows](https://superuser.com/questions/949560/how-do-i-set-system-environment-variables-in-windows-10), [Bash](https://askubuntu.com/questions/58814/how-do-i-add-environment-variables), and [PowerShell](https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_environment_variables?view=powershell-7). If you'd like to track this feature request in EF, the Github issue is [here](https://github.com/dotnet/efcore/issues/8695).

When using this, make sure your `UseInMemoryDatabase` setting is set to `false`. This should be the case by default on all environments that are not `Development`.

First, make sure you have `dotnet ef` installed. To install it globally run:

```shell
dotnet tool install -g dotnet-ef
```

Next, you'll want to perform your first migration. for example:

```shell
dotnet ef migrations add "InitialMigration" --project Infrastructure.Persistence --startup-project WebApi --output-dir Migrations
```

To add a configuration, you'll want to do this, otherwise the migration will look at your in memory db and fail:

```shell
dotnet ef migrations add "InitialMigration" --project Infrastructure.Persistence --startup-project WebApi --output-dir Migrations --configuration Release
```

To make updates to your database, you can run this:

```shell
dotnet ef database update -p Infrastructure.Persistence -s WebApi
```

### Common Db Migration Error

If you're getting an error like the below, make sure that your connection string is set in your `appsettings.json` file and `UseInMemoryDatabase` is set to `false`.

```
Unable to resolve service for type 'Microsoft.EntityFrameworkCore.Migrations.IMigrator'. This is often because no database provider has been configured for this DbContext. A provider can be configured by overriding the DbContext.OnConfiguring method or by using AddDbContext on the application service provider. If AddDbContext is used, then also ensure that your DbContext type accepts a DbContextOptions<TContext> object in its constructor and passes it to the base constructor for DbContext.
```



## Running Craftsman Locally

For those that want to contribute to the project, you'll need to run this locally to test out your updates. 

1. Clone the project repo

2. `cd` into the project directory and go to the `Craftsman` directory within it, for example:

   ```shell
   cd C:\Users\Paul\Documents\repos\Craftsman\Craftsman
   ```

3. Note that you can debug this project by clicking on the arrow next to `Run` button in Visual Studio and selecting `Crafstman Debug Properties`. From there, you can go to `Application Arguments` and enter the arguments you want. For example, something like `new:api C:\fullpath\filename.yaml` 

4. You'll also want to update file paths in `Program.cs` to whatever local values you'll want to use. We need to do this because in order to debug the project (or run it locally at all) we need to use `dotnet run` as described in step 2. If you know an easier way to deal with this I'd love to know! 

## Reference

For details on how to build a dotnet tool, see [here](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-create) for the MS docs. For another reference, [here](https://github.com/maartenba/dotnetcli-init) is a small example repo and a blog post about the project [here](https://blog.maartenballiauw.be/post/2017/04/10/extending-dotnet-cli-with-custom-tools.html).