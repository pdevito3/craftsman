namespace Craftsman.Removers
{
    using Craftsman.Helpers;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public static class EntityRemover
    {
        public static void Remove(string solutionDirectory, string filename)
        {
            var classPath = ClassPathHelper.EntityClassPath(solutionDirectory, filename);

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            File.Delete(classPath.FullClassPath);
        }
    }
}
