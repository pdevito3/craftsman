namespace Craftsman.Builders.Dtos
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using Domain;
    using Domain.Enums;
    using Services;

    public class DtoModifier
    {
        private readonly IFileSystem _fileSystem;

        public DtoModifier(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void AddPropertiesToDtos(string solutionDirectory, string entityName, List<EntityProperty> props, string projectBaseName)
        {
            UpdateDtoFile(solutionDirectory, entityName, props, Dto.Read, projectBaseName);
            UpdateDtoFile(solutionDirectory, entityName, props, Dto.Manipulation, projectBaseName);
        }

        private void UpdateDtoFile(string solutionDirectory, string entityName, List<EntityProperty> props, Dto dto, string projectBaseName)
        {
            var dtoFileName = $"{FileNames.GetDtoName(entityName, dto)}.cs";
            var classPath = ClassPathHelper.DtoClassPath(solutionDirectory, dtoFileName, entityName, projectBaseName);

            if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!_fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
            {
                using var output = _fileSystem.File.CreateText(tempPath);
                {
                    string line;

                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains($"add-on property marker"))
                        {
                            newText += @$"{Environment.NewLine}{Environment.NewLine}{DtoFileTextGenerator.DtoPropBuilder(props, dto)}";
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
