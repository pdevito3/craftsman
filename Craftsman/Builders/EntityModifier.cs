namespace Craftsman.Builders;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Domain;
using Services;

public class EntityModifier
{
    private readonly IFileSystem _fileSystem;

    public EntityModifier(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    public void AddEntityProperties(string solutionDirectory, string entityName, string entityPlural, List<EntityProperty> props, string projectBaseName)
    {
        var classPath = ClassPathHelper.EntityClassPath(solutionDirectory, $"{entityName}.cs", entityPlural, projectBaseName);

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

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
                        newText += @$"{Environment.NewLine}{Environment.NewLine}{EntityBuilder.EntityPropBuilder(props, entityName)}";
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
