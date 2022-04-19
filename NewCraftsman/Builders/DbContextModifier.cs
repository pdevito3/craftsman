namespace NewCraftsman.Builders
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class DbContextModifier
    {
        public static void AddDbSet(string solutionDirectory, List<Entity> entities, string dbContextName, string projectBaseName)
        {
            var classPath = ClassPathHelper.DbContextClassPath(solutionDirectory, $"{dbContextName}.cs", projectBaseName);
            var entitiesUsings = "";
            foreach (var entity in entities)
            {
                var entityClassPath = ClassPathHelper.EntityClassPath(solutionDirectory, "", entity.Plural, projectBaseName);
                entitiesUsings += $"using {entityClassPath.ClassNamespace};{Environment.NewLine}"; // note this foreach adds newline after where dbbuilder adds before
            }

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

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
                        if (line.Contains($"#region DbSet Region"))
                        {
                            newText += @$"{Environment.NewLine}{DbContextBuilder.GetDbSetText(entities)}";
                        }
                        
                        // TODO add test. assumes that this using exists and that the builder above adds a new line after the usings
                        if (line.Contains("using Microsoft.EntityFrameworkCore;"))
                        {
                            newText = $"{entitiesUsings}{line}";
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);
        }
    }
}
