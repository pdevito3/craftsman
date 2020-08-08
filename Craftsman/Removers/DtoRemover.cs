namespace Craftsman.Removers
{
    using Craftsman.Helpers;
    using System;
    using System.IO;

    public class DtoRemover
    {
        public static void RemoveDirectory(string solutionDirectory, string entityName)
        {
            var classPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entityName); // deleting directory, so I don't need to give a meaningful filename

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            var dir = new DirectoryInfo(classPath.ClassDirectory);
            dir.Delete(true);
        }
    }
}
