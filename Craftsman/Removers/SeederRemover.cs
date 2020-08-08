namespace Craftsman.Removers
{
    using Craftsman.Helpers;
    using System;
    using System.IO;

    public class SeederRemover
    {
        public static void RemoveDirectory(string solutionDirectory)
        {
            var classPath = ClassPathHelper.SeederClassPath(solutionDirectory, ""); // deleting directory, so I don't need to give a meaningful filename

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            var dir = new DirectoryInfo(classPath.ClassDirectory);
            dir.Delete(true);
        }
    }
}
