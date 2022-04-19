namespace NewCraftsman.Builders
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class EntityModifier
    {
        public static void AddEntityProperties(string solutionDirectory, string entityName, string entityPlural, List<EntityProperty> props, string projectBaseName)
        {
            var classPath = ClassPathHelper.EntityClassPath(solutionDirectory, $"{entityName}.cs", entityPlural, projectBaseName);

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
                        if (line.Contains($"add-on property marker"))
                        {
                            newText += @$"{Environment.NewLine}{Environment.NewLine}{EntityBuilder.EntityPropBuilder(props)}";
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
