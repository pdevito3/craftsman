namespace Craftsman.Builders
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

    public class SeederBuilder
    {
        public static void AddSeeders(string solutionDirectory, ApiTemplate template)
        {
            try
            {
                //TODO move these to a dictionary to lookup and overwrite if I want
                var seederTopPath = "Infrastructure.Persistence\\Seeders";
                var seederNamespace = seederTopPath.Replace("\\", ".");

                foreach(var entity in template.Entities)
                {
                    var seederDir = Path.Combine(solutionDirectory, seederTopPath);
                    if (!Directory.Exists(seederDir))
                        Directory.CreateDirectory(seederDir);

                    var pathString = Path.Combine(seederDir, $"{Utilities.GetSeederName(entity)}.cs");
                    if (File.Exists(pathString))
                        throw new FileAlreadyExistsException(pathString);

                    using (FileStream fs = File.Create(pathString))
                    {
                        var data = GetSeederFileText(seederNamespace, entity, template);
                        fs.Write(Encoding.UTF8.GetBytes(data));
                    }
                    
                    WriteInfo($"A new '{entity.Name}' seeder file was added here: {pathString}.");
                }

                RegisterAllSeeders(solutionDirectory, template);
            }
            catch (FileAlreadyExistsException)
            {
                WriteError("This file alread exists. Please enter a valid file path.");
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occured when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        public static string GetSeederFileText(string classNamespace, Entity entity, ApiTemplate template)
        {
            return @$"namespace {classNamespace}
{{

    using AutoBogus;
    using Domain.Entities;
    using Infrastructure.Persistence.Contexts;
    using System.Linq;

    public static class {Utilities.GetSeederName(entity)}
    {{
        public static void SeedSample{entity.Name}Data({template.DbContext.ContextName} context)
        {{
            if (!context.{entity.Plural}.Any())
            {{
                context.{entity.Plural}.Add(new AutoFaker<{entity.Name}>());
                context.{entity.Plural}.Add(new AutoFaker<{entity.Name}>());
                context.{entity.Plural}.Add(new AutoFaker<{entity.Name}>());

                context.SaveChanges();
            }}
        }}
    }}
}}";
        }

        public static void RegisterAllSeeders(string solutionDirectory, ApiTemplate template)
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
                        if (line.Contains("#region Entity Context Region"))
                        {
                            newText += @$"{Environment.NewLine}{GetSeederContextText(template)}";
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original nape
            File.Delete(pathString);
            File.Move(tempPath, pathString);
            WriteWarning($"TODO Need a message for the update of Startup.");
        }

        public static string GetSeederContextText(ApiTemplate template)
        {
            var seeders = "";
            foreach(var entity in template.Entities)
            {
                seeders += @$"
                    {Utilities.GetSeederName(entity)}.SeedSample{entity.Name}Data(app.ApplicationServices.GetService<{template.DbContext.ContextName}>());";
            }
            return $@"
                using (var context = app.ApplicationServices.GetService<{template.DbContext.ContextName}>())
                {{
                    context.Database.EnsureCreated();
                    {seeders}
                }}
";
        }
    }
}
