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
                        newText = @$"    public IReadOnlyCollection<{parentEntityName}> {parentEntityPlural} {{ get; }} = new List<{parentEntityName}>();{Environment.NewLine}{Environment.NewLine}{line}";
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

    public void AddParentRelationshipEntity(string srcDirectory, 
        EntityProperty property,
        string parentEntityName,
        string parentEntityPlural, 
        string projectBaseName)
    {
        var parentClassPath = ClassPathHelper.EntityClassPath(srcDirectory, $"{property.ForeignEntityName}.cs", property.ForeignEntityPlural, projectBaseName);
        var classPath = ClassPathHelper.EntityClassPath(srcDirectory, $"{parentEntityName}.cs", parentEntityPlural, projectBaseName);

        if (property.IsChildRelationship)
        {
            parentClassPath = ClassPathHelper.EntityClassPath(srcDirectory, $"{parentEntityName}.cs", parentEntityPlural, projectBaseName);
            classPath = ClassPathHelper.EntityClassPath(srcDirectory, $"{property.ForeignEntityName}.cs", property.ForeignEntityPlural, projectBaseName);
        }
        
        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
        {
            _consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
            return;
        }
        
        var parentUsingStatement = $@"using {parentClassPath.ClassNamespace};";
        var usingStatementHasBeenAdded = false;
        var propToAdd = property.GetDbRelationship.GetPrincipalPropString(property.Type,
            property.Name,
            null,
            property.ForeignEntityName,
            property.ForeignEntityPlural,
            parentEntityName,
            parentEntityPlural);
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
                        newText = @$"{propToAdd}{Environment.NewLine}{line}";
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

    public void AddEntityManyManagementMethods(string srcDirectory, 
        EntityProperty property,
        string parentEntityName,
        string parentEntityPlural,
        string projectBaseName)
    {
        var classPath = ClassPathHelper.EntityClassPath(srcDirectory, $"{parentEntityName}.cs", parentEntityPlural, projectBaseName);

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
        {
            _consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
            return;
        }
        
        var managedListMethods = "";
        var managedEntity = property.ForeignEntityName;
        managedListMethods += GetListManagementMethods(parentEntityName, managedEntity, property.ForeignEntityPlural);
        
        // var managedEntityMethod = "";
        // var manyToOne = entity.Properties.Where(x => x.GetDbRelationship.IsManyToOne || x.GetDbRelationship.IsOneToOne).ToList();
        // foreach (var oneToManyProp in manyToOne)
        // {
        //     var managedEntity = oneToManyProp.ForeignEntityName;
        //     var managedPropName = oneToManyProp.Name;
        //     managedEntityMethod += GetEntityManagementMethods(entity.Name, managedEntity, managedPropName);
        // }
        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"Add Prop Methods Marker"))
                    {
                        newText = @$"{managedListMethods}{line}";
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }

    public void AddEntitySingularManagementMethods(string srcDirectory, 
        EntityProperty property,
        string parentEntityName,
        string parentEntityPlural,
        string projectBaseName)
    {
        var classPath = ClassPathHelper.EntityClassPath(srcDirectory, $"{parentEntityName}.cs", parentEntityPlural, projectBaseName);

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
        {
            _consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
            return;
        }
        
        var managedEntityMethod = "";
        var managedEntity = property.ForeignEntityName;
        var managedPropName = property.Name;
        managedEntityMethod += GetEntityManagementMethods(parentEntityName, managedEntity, managedPropName);
        
        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"Add Prop Methods Marker"))
                    {
                        newText = @$"{managedEntityMethod}{line}";
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }

    private static string GetListManagementMethods(string rootEntity, string managedEntity, string managedEntityPlural)
    {
        var lowerManagedEntity = managedEntity.LowercaseFirstLetter();
        var lowerManagedEntityPlural = managedEntityPlural.LowercaseFirstLetter();
        return $@"    public {rootEntity} Add{managedEntity}({managedEntity} {lowerManagedEntity})
    {{
        _{lowerManagedEntityPlural}.Add({lowerManagedEntity});
        return this;
    }}
    
    public {rootEntity} Remove{managedEntity}({managedEntity} {lowerManagedEntity})
    {{
        _{lowerManagedEntityPlural}.RemoveAll(x => x.Id == {lowerManagedEntity}.Id);
        return this;
    }}

";
    }

    private static string GetEntityManagementMethods(string rootEntity, string managedEntity, string managedPropName)
    {
        var lowerManagedEntity = managedEntity.LowercaseFirstLetter();
        return $@"    public {rootEntity} Set{managedEntity}({managedEntity} {lowerManagedEntity})
    {{
        {managedPropName} = {lowerManagedEntity};
        return this;
    }}

";
    }
}
