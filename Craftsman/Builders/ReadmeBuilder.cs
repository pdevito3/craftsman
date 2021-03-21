namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class ReadmeBuilder
    {
        public static void CreateReadme(string solutionDirectory, string solutionName, IFileSystem fileSystem)
        {
            try
            {
                var classPath = ClassPathHelper.SolutionClassPath(solutionDirectory, $"README.md");

                if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                    fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

                if (fileSystem.File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (var fs = fileSystem.File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetReadmeFileText(solutionName);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{fileSystem.Path.DirectorySeparatorChar}", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        public static void CreateBoundedContextReadme(string solutionDirectory, string solutionName, IFileSystem fileSystem)
        {
            try
            {
                var classPath = ClassPathHelper.SolutionClassPath(solutionDirectory, $"README.md");

                if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                    fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

                if (fileSystem.File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (var fs = fileSystem.File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetBoundedContextReadmeFileText(solutionName, solutionDirectory);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{fileSystem.Path.DirectorySeparatorChar}", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        public static string GetReadmeFileText(string solutionName)
        {
            return @$"# {solutionName}

This project was created with [Craftsman](https://github.com/pdevito3/craftsman).

Each bounded context is in this directory and has additional information on how to run each project in their respective README files.";
        }

        public static string GetBoundedContextReadmeFileText(string solutionName, string srcDir)
        {
            return @$"# {solutionName}

This project was created with [Craftsman](https://github.com/pdevito3/craftsman).

## Get Started

Go to your solution directory:

```shell
cd {solutionName}
```

Run your solution:

```shell
dotnet run --project webapi
```

## Running Integration Tests
To run integration tests:

1. Ensure that you have docker installed.
2. Go to your src directory: `cd {srcDir}`
3. Set an environment. It doesn't matter what that environment name is for these purposes.
    - Powershell: `$Env:ASPNETCORE_ENVIRONMENT = ""IntegrationTesting""`
    - Bash: export `ASPNETCORE_ENVIRONMENT = IntegrationTesting`
4. Run a Migration (necessary to set up the database) `dotnet ef migrations add ""InitialMigration"" --project {solutionName}.Infrastructure --startup-project {solutionName}.WebApi --output-dir Migrations`
5. Run the tests. They will take some time on the first run in the last 24 hours in order to set up the docker configuration.
";
        }
    }
}
