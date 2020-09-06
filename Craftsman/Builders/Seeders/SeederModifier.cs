namespace Craftsman.Builders.Seeders
{
    using Craftsman.Builders.Dtos;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class SeederModifier
    {
        public static void AddSeeders(string solutionDirectory, ApiTemplate template)
        {
            try
            {
                foreach (var entity in template.Entities)
                {
                    var classPath = ClassPathHelper.SeederClassPath(solutionDirectory, $"{Utilities.GetSeederName(entity)}.cs");

                    if (!Directory.Exists(classPath.ClassDirectory))
                        Directory.CreateDirectory(classPath.ClassDirectory);

                    if (File.Exists(classPath.FullClassPath))
                        throw new FileAlreadyExistsException(classPath.FullClassPath);

                    using (FileStream fs = File.Create(classPath.FullClassPath))
                    {
                        var data = SeederFunctions.GetSeederFileText(classPath.ClassNamespace, entity, template);
                        fs.Write(Encoding.UTF8.GetBytes(data));
                    }

                    GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
                }

                RegisterAllNewSeeders(solutionDirectory, template);
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        private static void RegisterAllNewSeeders(string solutionDirectory, ApiTemplate template)
        {
            //TODO move these to a dictionary to lookup and overwrite if I want
            var repoTopPath = "WebApi";

            var entityDir = Path.Combine(solutionDirectory, repoTopPath);
            if (!Directory.Exists(entityDir))
                throw new DirectoryNotFoundException($"The `{entityDir}` directory could not be found.");

            var pathString = Path.Combine(entityDir, $"Startup.cs");
            if (!File.Exists(pathString))
                throw new FileNotFoundException($"The `{pathString}` file could not be found.");

            var tempPath = $"{pathString}temp";
            using (var input = File.OpenText(pathString))
            {
                using (var output = new StreamWriter(tempPath))
                {
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains($"#region {template.DbContext.ContextName} Seeder Region"))
                        {
                            newText += @$"{Environment.NewLine}{GetSeederContextText(template)}";
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(pathString);
            File.Move(tempPath, pathString);

            GlobalSingleton.AddUpdatedFile(pathString.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
        }

        private static string GetSeederContextText(ApiTemplate template)
        {
            var seeders = "";
            foreach (var entity in template.Entities)
            {
                seeders += @$"
                    {Utilities.GetSeederName(entity)}.SeedSample{entity.Name}Data(app.ApplicationServices.GetService<{template.DbContext.ContextName}>());";
            }

            return seeders;
        }
    }
}
