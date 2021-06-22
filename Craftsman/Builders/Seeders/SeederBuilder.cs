namespace Craftsman.Builders.Seeders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class SeederBuilder
    {
        public static void AddSeeders(string solutionDirectory, List<Entity> entities, string dbContextName, string projectBaseName)
        {
            foreach (var entity in entities)
            {
                var classPath = ClassPathHelper.SeederClassPath(solutionDirectory, $"{Utilities.GetSeederName(entity)}.cs", projectBaseName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = SeederFunctions.GetEntitySeederFileText(classPath.ClassNamespace, entity, dbContextName, solutionDirectory, projectBaseName);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }
            }

            RegisterAllSeeders(solutionDirectory, entities, dbContextName, projectBaseName);
        }

        private static void RegisterAllSeeders(string solutionDirectory, List<Entity> entities, string dbContextName, string projectBaseName)
        {
            var classPath = ClassPathHelper.StartupClassPath(solutionDirectory, $"StartupDevelopment.cs", projectBaseName);
            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using (var output = new StreamWriter(tempPath))
                {
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains("Entity Context - Do Not Delete"))
                        {
                            newText += @$"{Environment.NewLine}{GetSeederContextText(entities, dbContextName)}";
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);
        }

        private static string GetSeederContextText(List<Entity> entities, string dbContextName)
        {
            var seeders = "";
            foreach (var entity in entities)
            {
                seeders += @$"
                    {Utilities.GetSeederName(entity)}.SeedSample{entity.Name}Data(app.ApplicationServices.GetService<{dbContextName}>());";
            }
            return $@"
            using (var context = app.ApplicationServices.GetService<{dbContextName}>())
            {{
                context.Database.EnsureCreated();

                // {dbContextName} Seeders
{seeders.RemoveLastNewLine()}
            }}
";
        }
    }
}