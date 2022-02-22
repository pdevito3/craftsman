namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System.IO.Abstractions;
    using System.Text;

    public class ReadmeBuilder
    {
        public static void CreateReadme(string solutionDirectory, string domainName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.SolutionClassPath(solutionDirectory, $"README.md");

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (var fs = fileSystem.File.Create(classPath.FullClassPath))
            {
                var data = "";
                data = GetReadmeFileText(domainName);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static string GetReadmeFileText(string domainName)
        {
            return @$"# {domainName}

This project was created with [Craftsman](https://github.com/pdevito3/craftsman).

## Getting Started
### Set Up Your Database
This project is configured to reference a live database instead of an in-memory one for more robust development. 
By default, the database will be configured to run in a docker container and already has the connection 
string configured in your launch settings.

To set up your database you can either:
1. Run `docker-compose up --build` to just spin up your database(s) (and message broker, if applicable).
2. Run `docker-compose -f docker-compose.all.yaml up --build` to spin up your database(s) along with all of your apis. 

After you have your database(s) running in docker, make sure you apply your migrations:
1. `cd` to the boundary project root (e.g. `cd RecipeManagement/src/RecipeManagement`
2. Run `dotnet ef database update` to apply your migrations

### Running Your Apis
If you used `docker-compose.all.yaml` your api(s) will be running in docker containers; these settings will be set in your compose file. 
Otherwise, you can use the `dotnet run` command or the built in `Run` option in your IDE. Either way, those will be getting all their 
settings from `launchSettings.json`.

## Running Integration Tests
To run integration tests:

1. Ensure that you have docker installed.
2. Go to your src directory for the bounded context that you want to test.
3. Confirm that you have migrations in your infrastructure project. If you need to add them, see the [instructions below](#running-migrations).
4. Run the tests

> ⏳ If you don't have the database image pulled down to your machine, they will take some time on the first run.

### Troubleshooting
-If you have trouble with your tests, try removing the container and volume marked for your integration tests.
- If your entity has foreign keys, you'll likely need to adjust some of your tests after scaffolding to accomodate them.

## Running Migrations

To create a new migration, make sure your environment is *not* set to `Development`:

### Powershell
```powershell
$Env:ASPNETCORE_ENVIRONMENT = ""anything""
```

### Bash
```bash
export ASPNETCORE_ENVIRONMENT=anything
```

Then run the following:

```shell
cd YourBoundedContextName/src/YourBoundedContextName
dotnet ef migrations add ""your-description-here""
```

To apply your migrations to your local db, run the following:

```bash
cd YourBoundedContextName/src
dotnet ef database update --project YourBoundedContextName
```
";
        }
    }
}