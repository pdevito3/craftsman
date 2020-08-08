namespace Craftsman.Removers
{
    using Craftsman.Helpers;
    using System;
    using System.IO;

    public class DbContextRemover
    {
        public static void RemoveDirectory(string solutionDirectory)
        {
            var classPath = ClassPathHelper.DbContextClassPath(solutionDirectory, ""); // deleting directory, so I don't need to give a meaningful filename

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            var dir = new DirectoryInfo(classPath.ClassDirectory);
            dir.Delete(true);
        }
    }
}
