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

    public void AddStringArrayProperty(string srcDirectory, 
        string entityName, 
        EntityProperty entityProperty,
        DbProvider dbProvider,
        string projectBaseName)
    {
        var classPath = ClassPathHelper.DatabaseConfigClassPath(srcDirectory,
            $"{FileNames.GetDatabaseEntityConfigName(entityName)}.cs",
            projectBaseName);

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
        {
            _consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
            return;
        }

        if (dbProvider != DbProvider.Postgres)
        {
            if (dbProvider == DbProvider.Unknown)
            {
                _consoleWriter.WriteWarning(@$"Automatic database configurations for string arrays are only 
supported for Postgres databases. The current db provider is unknown, so you may need to manually resolve your db config 
for {entityProperty.Name} in the {classPath.ClassName} class if you are not using Postgres.");
            }
            else
            {
                _consoleWriter.WriteWarning(@$"Automatic database configurations for string arrays are only 
supported for Postgres databases. You will need to manually resolve your db config 
for {entityProperty.Name} in the {classPath.ClassName} class.");
            }
        }
        
        var stringArrayProps = $@"
        builder.Property(x => x.{entityProperty.Name}).HasColumnType(""text[]"");";

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
                        newText += stringArrayProps;
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
