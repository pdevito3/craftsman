namespace Craftsman.Builders
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public static class SolutionBuilder
    {
        public static void BuildSolution(string solutionDirectory, ApiTemplate template)
        {
            var separator = Path.DirectorySeparatorChar;
            Directory.CreateDirectory(solutionDirectory);
            Directory.CreateDirectory($"{solutionDirectory}{separator}Application");
            Directory.CreateDirectory($"{solutionDirectory}{separator}Domain");
            Directory.CreateDirectory($"{solutionDirectory}{separator}Infrastructure.Persistence");
            Directory.CreateDirectory($"{solutionDirectory}{separator}Infrastructure.Shared");
            Directory.CreateDirectory($"{solutionDirectory}{separator}WebApi");

            Utilities.ExecuteProcess("dotnet", @$"new sln -n {template.SolutionName}", solutionDirectory);
        }
    }
}
