namespace Craftsman.Builders;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Domain;
using Helpers;
using Services;
using MediatR;

public static class DatabaseEntityConfigModifier
{
    public sealed record AddRelationshipsCommand(string EntityName, string EntityPlural, EntityProperty EntityProperty, string ProjectBaseName) : IRequest;
    public sealed record AddStringArrayPropertyCommand(string EntityName, EntityProperty EntityProperty, DbProvider DbProvider, string ProjectBaseName) : IRequest;
    public sealed record AddValueObjectConfigCommand(string EntityName, EntityProperty EntityProperty, string ProjectBaseName) : IRequest;

    public class AddRelationshipsHandler(IFileSystem fileSystem, IConsoleWriter consoleWriter, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<AddRelationshipsCommand>
    {
        public Task Handle(AddRelationshipsCommand request, CancellationToken cancellationToken)
        {
            AddRelationships(scaffoldingDirectoryStore.SrcDirectory, request.EntityName, request.EntityPlural, request.EntityProperty, request.ProjectBaseName, fileSystem, consoleWriter);
            return Task.CompletedTask;
        }
    }

    public class AddStringArrayPropertyHandler(IFileSystem fileSystem, IConsoleWriter consoleWriter, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<AddStringArrayPropertyCommand>
    {
        public Task Handle(AddStringArrayPropertyCommand request, CancellationToken cancellationToken)
        {
            AddStringArrayProperty(scaffoldingDirectoryStore.SrcDirectory, request.EntityName, request.EntityProperty, request.DbProvider, request.ProjectBaseName, fileSystem, consoleWriter);
            return Task.CompletedTask;
        }
    }

    public class AddValueObjectConfigHandler(IFileSystem fileSystem, IConsoleWriter consoleWriter, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<AddValueObjectConfigCommand>
    {
        public Task Handle(AddValueObjectConfigCommand request, CancellationToken cancellationToken)
        {
            AddValueObjectConfig(scaffoldingDirectoryStore.SrcDirectory, request.EntityName, request.EntityProperty, request.ProjectBaseName, fileSystem, consoleWriter);
            return Task.CompletedTask;
        }
    }

    private static void AddRelationships(string srcDirectory, string entityName, string entityPlural, EntityProperty entityProperty, string projectBaseName, IFileSystem fileSystem, IConsoleWriter consoleWriter)
    {       
        var classPath = ClassPathHelper.DatabaseConfigClassPath(srcDirectory, 
            $"{FileNames.GetDatabaseEntityConfigName(entityName)}.cs",
            projectBaseName);
        
        if(entityProperty.IsChildRelationship)
            classPath = ClassPathHelper.DatabaseConfigClassPath(srcDirectory, 
                $"{FileNames.GetDatabaseEntityConfigName(entityProperty.ForeignEntityName)}.cs",
                projectBaseName);

        if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
            fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!fileSystem.File.Exists(classPath.FullClassPath))
        {
            consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
            return;
        }
            
        var relationshipConfigs = string.Empty;
        relationshipConfigs += entityProperty.GetDbRelationship.GetEntityDbConfig(entityName, entityPlural, 
            entityProperty.Name, entityProperty.ForeignEntityPlural, entityProperty.ForeignEntityName);
        
        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = fileSystem.File.CreateText(tempPath);
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
        fileSystem.File.Delete(classPath.FullClassPath);
        fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }

    private static void AddStringArrayProperty(string srcDirectory, 
        string entityName, 
        EntityProperty entityProperty,
        DbProvider dbProvider,
        string projectBaseName,
        IFileSystem fileSystem,
        IConsoleWriter consoleWriter)
    {
        var classPath = ClassPathHelper.DatabaseConfigClassPath(srcDirectory,
            $"{FileNames.GetDatabaseEntityConfigName(entityName)}.cs",
            projectBaseName);

        if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
            fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!fileSystem.File.Exists(classPath.FullClassPath))
        {
            consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
            return;
        }

        if (dbProvider != DbProvider.Postgres)
        {
            if (dbProvider == DbProvider.Unknown)
            {
                consoleWriter.WriteWarning(@$"Automatic database configurations for string arrays are only 
supported for Postgres databases. The current db provider is unknown, so you may need to manually resolve your db config 
for {entityProperty.Name} in the {classPath.ClassName} class if you are not using Postgres.");
            }
            else
            {
                consoleWriter.WriteWarning(@$"Automatic database configurations for string arrays are only 
supported for Postgres databases. You will need to manually resolve your db config 
for {entityProperty.Name} in the {classPath.ClassName} class.");
            }
        }
        
        var stringArrayProps = $@"
        builder.Property(x => x.{entityProperty.Name}).HasColumnType(""text[]"");";

        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"Property Marker --"))
                    {
                        newText += stringArrayProps;
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        fileSystem.File.Delete(classPath.FullClassPath);
        fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }

    private static void AddValueObjectConfig(string srcDirectory, 
        string entityName,
        EntityProperty entityProperty,
        string projectBaseName,
        IFileSystem fileSystem,
        IConsoleWriter consoleWriter)
    {
        var classPath = ClassPathHelper.DatabaseConfigClassPath(srcDirectory,
            $"{FileNames.GetDatabaseEntityConfigName(entityName)}.cs",
            projectBaseName);

        var voClassPath = ClassPathHelper.EntityClassPath(srcDirectory,
            $"{FileNames.GetMappingName(entityName)}.cs",
            entityProperty.ValueObjectPlural,
            projectBaseName);
        
        if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
            fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!fileSystem.File.Exists(classPath.FullClassPath))
        {
            consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
            return;
        }

        var usingAdded = false;
        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"Property Marker --"))
                    {
                        newText += entityProperty.ValueObjectType.GetDbConfig(entityProperty.Name);
                    }
                    if (line.Contains($"using ") && !usingAdded)
                    {
                        newText += $@"{Environment.NewLine}using {voClassPath.ClassNamespace};";
                        usingAdded = true;
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        fileSystem.File.Delete(classPath.FullClassPath);
        fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }
}
