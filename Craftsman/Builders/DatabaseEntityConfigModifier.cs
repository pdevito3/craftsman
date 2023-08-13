namespace Craftsman.Builders;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Domain;
using Helpers;
using Services;

public class DatabaseEntityConfigModifier
{
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;

    public DatabaseEntityConfigModifier(IFileSystem fileSystem, IConsoleWriter consoleWriter)
    {
        _fileSystem = fileSystem;
        _consoleWriter = consoleWriter;
    }

    public void AddRelationships(string srcDirectory, string entityName, string entityPlural, EntityProperty entityProperty, string projectBaseName)
    {       
        var classPath = ClassPathHelper.DatabaseConfigClassPath(srcDirectory, 
            $"{FileNames.GetDatabaseEntityConfigName(entityName)}.cs",
            projectBaseName);
        
        if(entityProperty.IsChildRelationship)
            classPath = ClassPathHelper.DatabaseConfigClassPath(srcDirectory, 
                $"{FileNames.GetDatabaseEntityConfigName(entityProperty.ForeignEntityName)}.cs",
                projectBaseName);

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
        {
            _consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
            return;
        }
            
        var relationshipConfigs = string.Empty;
        relationshipConfigs += entityProperty.GetDbRelationship.GetEntityDbConfig(entityName, entityPlural, 
            entityProperty.Name, entityProperty.ForeignEntityPlural, entityProperty.ForeignEntityName);
        
        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"Relationship Marker --"))
                    {
                        newText += relationshipConfigs;
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
