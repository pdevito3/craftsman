namespace Craftsman.Removers
{
    using Craftsman.Builders;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class StartupCleaner
    {
        // will probably want to refactor this in the future when there is > 1 startup based on env
        public static void CleanStartup(string solutionDirectory, ApiTemplate template)
        {
            var classPath = ClassPathHelper.StartupClassPath(solutionDirectory, "Startup.cs");

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            File.Delete(classPath.FullClassPath);

            StartupBuilder.CreateStartup(solutionDirectory, "Startup", template);
        }
    }
}
