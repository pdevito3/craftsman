namespace Craftsman.Removers
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;

    public class TestIntegrationRemover
    {
        public static void RemoveDirectory(string solutionDirectory, string entityName, ApiTemplate template)
        {
            var classPath = ClassPathHelper.TestIntegrationClassPath(solutionDirectory, $"", entityName, template.SolutionName); // deleting directory, so I don't need to give a meaningful filename

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            var dir = new DirectoryInfo(classPath.ClassDirectory);
            dir.Delete(true);
        }
    }
}
