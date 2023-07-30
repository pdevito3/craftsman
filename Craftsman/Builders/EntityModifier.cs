namespace Craftsman.Builders;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Domain;
using Helpers;
using Services;

public class EntityModifier
{
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;

    public EntityModifier(IFileSystem fileSystem, IConsoleWriter consoleWriter)
    {
        _fileSystem = fileSystem;
        _consoleWriter = consoleWriter;
    }
    public void AddEntityProperties(string srcDirectory, string entityName, string entityPlural, List<EntityProperty> props, string projectBaseName)
    {
        var classPath = ClassPathHelper.EntityClassPath(srcDirectory, $"{entityName}.cs", entityPlural, projectBaseName);

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
                        newText += @$"{Environment.NewLine}{Environment.NewLine}{EntityBuilder.EntityPropBuilder(props)}";
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }

    public void AddSingularRelationshipEntity(string srcDirectory, 
        string childEntityName, 
        string childEntityPlural, 
        string parentEntityName,
        string parentEntityPlural, 
        string projectBaseName)
    {
        var classPath = ClassPathHelper.EntityClassPath(srcDirectory, $"{childEntityName}.cs", childEntityPlural, projectBaseName);
        var parentClassPath = ClassPathHelper.EntityClassPath(srcDirectory, $"{parentEntityName}.cs", parentEntityPlural, projectBaseName);

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
        {
            _consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
            return;
        }
        
        var parentUsingStatement = $@"using {parentClassPath.ClassNamespace};";
        var usingStatementHasBeenAdded = false;
        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"Add Props Marker"))
                    {
                        newText = @$"    public {parentEntityName} {parentEntityName} {{ get; }}{Environment.NewLine}{Environment.NewLine}{line}";
                    }
                    if (line.Contains($"using") && !usingStatementHasBeenAdded)
                    {
                        newText += @$"{Environment.NewLine}{parentUsingStatement}";
                        usingStatementHasBeenAdded = true;
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }

    public void AddManyRelationshipEntity(string srcDirectory, 
        string childEntityName, 
        string childEntityPlural, 
        string parentEntityName,
        string parentEntityPlural, 
        string projectBaseName)
    {
        var classPath = ClassPathHelper.EntityClassPath(srcDirectory, $"{childEntityName}.cs", childEntityPlural, projectBaseName);
        var parentClassPath = ClassPathHelper.EntityClassPath(srcDirectory, $"{parentEntityName}.cs", parentEntityPlural, projectBaseName);

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
        {
            _consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
            return;
        }
        
        var parentUsingStatement = $@"using {parentClassPath.ClassNamespace};";
        var usingStatementHasBeenAdded = false;
        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"Add Props Marker"))
                    {
                        newText = @$"    public IReadOnlyCollection<{parentEntityName}> {parentEntityPlural} {{ get; }}{Environment.NewLine}{Environment.NewLine}{line}";
                    }
                    if (line.Contains($"using") && !usingStatementHasBeenAdded)
                    {
                        newText += @$"{Environment.NewLine}{parentUsingStatement}";
                        usingStatementHasBeenAdded = true;
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
