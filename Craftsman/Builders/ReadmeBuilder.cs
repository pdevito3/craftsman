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
        public static void CreateReadme(string solutionDirectory, ApiTemplate template, IFileSystem fileSystem)
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
                    data = GetReadmeFileText(template);
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

        public static string GetReadmeFileText(ApiTemplate template)
        {
            return @$"# {template.SolutionName}

This project was created with [Craftsman](https://github.com/pdevito3/craftsman).

## Get Started

Go to your solution directory:

```shell
cd {template.SolutionName}
```

Run your solution:

```shell
dotnet run --project webapi
```

";
        }
    }
}
