namespace Craftsman.Builders.Bff.Features.Dynamic.Api
{
    using System;
    using System.IO;
    using Enums;
    using Helpers;

    public class DynamicFeatureApiIndexModifier
    {
        public static void AddFeature(string spaDirectory, string entityName, FeatureType type)
        {
            var classPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, entityName, BffFeatureCategory.Api , "index.ts");
            var featureFilenameBase = Utilities.GetBffApiFilenameBase(entityName, type);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var tempPath = $"{classPath.FullClassPath}temp";
            var exportIsAddedToFile = false;
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using (var output = new StreamWriter(tempPath))
                {
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (!exportIsAddedToFile)
                        {
                            newText += @$"{Environment.NewLine}export * from './{featureFilenameBase}';";
                            exportIsAddedToFile = true;
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
