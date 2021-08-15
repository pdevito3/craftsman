namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System.IO.Abstractions;
    using System.Text;

    public class ReadmeBuilder
    {
        public static void CreateReadme(string solutionDirectory, string projectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.SolutionClassPath(solutionDirectory, $"README.md");

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (var fs = fileSystem.File.Create(classPath.FullClassPath))
            {
                var data = "";
                data = GetReadmeFileText(projectName);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static string GetReadmeFileText(string projectName)
        {
            return @$"# {projectName}

This project was created with [Craftsman](https://github.com/pdevito3/craftsman).

## Get Started

Go to your solution directory:

```shell
cd {projectName}
```

Run your solution:

```shell
dotnet run --project YourBoundedContextName.WebApi
```

## Running Integration Tests
To run integration tests:

1. Ensure that you have docker installed.
2. Go to your src directory for the bounded context that you want to test.
3. Confirm that you have migrations in your infrastructure project. If not you can add them by doing the following:
    1. Set an environment variable. It doesn't matter what that environment name is for these purposes.
        - Powershell: `$Env:ASPNETCORE_ENVIRONMENT = ""IntegrationTesting""`
        - Bash: export `ASPNETCORE_ENVIRONMENT = IntegrationTesting`
    2. Run a Migration:`dotnet ef migrations add ""InitialMigration"" --project YourBoundedContextName.Infrastructure --startup-project YourBoundedContextName.WebApi --output-dir Migrations`
4. Run the tests. They will take some time on the first run in the last 24 hours in order to set up the docker configuration.
";
        }
    }
}