namespace Craftsman.Builders
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class SolutionBuilder
    {
        public static void BuildSolution(string solutionDirectory, ApiTemplate template)
        {
            var separator = Path.DirectorySeparatorChar;
            var domainDirectory = $"{solutionDirectory}{separator}Domain";

            Directory.CreateDirectory(solutionDirectory);
            Directory.CreateDirectory($"{solutionDirectory}{separator}Application");
            Directory.CreateDirectory($"{solutionDirectory}{separator}Infrastructure.Persistence");
            Directory.CreateDirectory($"{solutionDirectory}{separator}Infrastructure.Shared");
            Directory.CreateDirectory($"{solutionDirectory}{separator}WebApi");

            try
            {
                Utilities.ExecuteProcess("dotnet", @$"new sln -n {template.SolutionName}", solutionDirectory);
            }
            catch(Exception e)
            {
                // must be using the .net 5 sdk
            }

            // add webapi first so it is default project?
            BuildDomainProject(solutionDirectory, domainDirectory);
            var stopper = true;
        }

        private static void BuildDomainProject(string solutionDirectory, string domainDirectory)
        {
            var separator = Path.DirectorySeparatorChar;

            // domain and app need to be added to a virtual `Common` folder, something like this: dotnet add path/to/project.csproj --solution-folder VirtualFolder
            Utilities.ExecuteProcess("dotnet", $@"new classlib -n Domain -f netstandard2.1", solutionDirectory);
            Utilities.ExecuteProcess("dotnet", $@"sln add ""Domain/Domain.csproj""", solutionDirectory);

            // this dir is not getting added to the projec though like you would if you created a new folder in vs...
            Directory.CreateDirectory($"{domainDirectory}{separator}Entities");

            // remove default class
            var class1FilePath = Path.Combine(domainDirectory, "Class1.cs");
            if (File.Exists(class1FilePath))
                File.Delete(class1FilePath);

            Utilities.ExecuteProcess("dotnet", $@"add Domain package Sieve", solutionDirectory);
        }
    }
}
