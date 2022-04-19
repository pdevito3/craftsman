namespace NewCraftsman.Builders
{
    using System;
    using System.IO;

    public class StartupModifier
    {
        public static void RegisterMassTransitService(string srcDirectory, string projectBaseName)
        {
            var classPath = Utilities.GetStartupClassPath(srcDirectory, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using var output = new StreamWriter(tempPath);
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"services.AddInfrastructure"))
                        newText += @$"{Environment.NewLine}        services.AddMassTransitServices(_config, _env);";

                    //if (line.Contains($@"{infraClassPath.ClassNamespace};"))
                    //    newText += @$"{ Environment.NewLine}    using { serviceRegistrationsClassPath.ClassNamespace}; ";

                    output.WriteLine(newText);
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);
        }
    }
}