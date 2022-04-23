namespace Craftsman.Builders.Bff.Features.Dynamic.Api
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using Domain.Enums;
    using Services;

    public class DynamicFeatureApiIndexModifier
    {
        private readonly IFileSystem _fileSystem;

        public DynamicFeatureApiIndexModifier(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void AddFeature(string spaDirectory, string entityName, string entityPlural, FeatureType type)
        {
            var classPath = ClassPathHelper.BffSpaFeatureClassPath(spaDirectory, entityPlural, BffFeatureCategory.Api , "index.ts");
            var featureFilenameBase = FileNames.GetBffApiFilenameBase(entityName, type);

            if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
                _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (!_fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var exportIsAddedToFile = false;
            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
            {
                using var output = _fileSystem.File.CreateText(tempPath);
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
            _fileSystem.File.Delete(classPath.FullClassPath);
            _fileSystem.File.Move(tempPath, classPath.FullClassPath);
        }
    }
}
