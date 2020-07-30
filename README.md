# Craftsman

## Description

This is a custom .NET Core CLI tool to scaffold out new files and projects.

## Prerequisites

- VS2019 .Net Core Feature Set or .Net Core 3.1 SDK https://dotnet.microsoft.com/download/dotnet-core/3.1

## How To Use

### Locally

1. Clone the project repo

2. `cd` into the project directory and go to the `Craftsman` directory within it, for example:

   ```shell
   λ cd C:\Users\Paul\Documents\repos\Craftsman\Craftsman
   ```

3. Set up a `yaml` or `json` file with the API template you want to create. For now, it only has a POC for creating an entity. For example:

   **JSON**

   ```json
   {
     "Entities": [
       {
         "Name": "Pet",
         "Properties": [
           {
             "Name": "Name",
             "Type": "string",
             "Filter": true
           },
           {
             "Name": "Type",
             "Type": "string",
             "Filter": true
           }
         ]
       }
     ]
   }
   ```

   **YAML**

   ```yml
   Entities:
   - Name: Pet
     Properties:
     - Name: Name
       Type: string
       Filter: true
     - Name: Type
       Type: string
       Filter: true
   ```

4. Run any of the following commands

   `dotnet run --api C:\fullpath\filename.json`
   `dotnet run --api C:\fullpath\filename.yml`
   `dotnet run -a C:\fullpath\filename.json`
   `dotnet run -a C:\fullpath\filename.yml`

5. If successful, the new entity file will be added to the project. When realistically using this, you would be doing it from the installed nuget package and it would be added to your actual project, but this works as well.

*Note that you can debug this project by clicking on the arrow next to `Run` button in Visual Studio and selecting 'Crafstman Debug Properties'. From there, you can go to 'Application Arguments' and enter the arguments you want. For example, something like `--api C:\fullpath\filename.json` or `--api -h`.*

### Installed Nuget Package

<u>This is not ready yet</u>, but once this is published to nuget you can run this operation more efficiently with the following:

1. Install the Craftsman package 

   ```shell
   dotnet tool install --global --add-source ./nupkg microsoft.craftsman
   ```

2. Run the below for a description of the API command

   - `craftsman --api -h` or `craftsman --api -help` for a list of all commands
   - `craftsman --api -h` or `craftsman --api -help` for a description of api commands
   - `craftsman --api C:\fullpath\filename.json` to scaffold an api
   - `craftsman --api C:\fullpath\filename.yml` to scaffold an api
   - `craftsman -a C:\fullpath\filename.json` to scaffold an api
   - `craftsman -a C:\fullpath\filename.yml` to scaffold an api

## Reference

For details on how to build a dotnet tool, see [here](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-create) for the MS docs. For another reference, [here](https://github.com/maartenba/dotnetcli-init) is a small example repo and a blog post about the project [here](https://blog.maartenballiauw.be/post/2017/04/10/extending-dotnet-cli-with-custom-tools.html).