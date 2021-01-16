namespace Craftsman.Removers
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;

    public class TestFakesRemover
    {
        public static void RemoveDirectory(string solutionDirectory, string entityName, string solutionName)
        {
            var classPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, $"", entityName, solutionName); // deleting directory, so I don't need to give a meaningful filename

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            var dir = new DirectoryInfo(classPath.ClassDirectory);
            dir.Delete(true);
        }
    }
}
