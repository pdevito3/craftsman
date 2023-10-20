namespace Craftsman.Builders;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Domain;
using Domain.Enums;
using Helpers;
using Humanizer;
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

    public void AddSingularRelationshipEntity(string srcDirectory, 
        string childEntityName, 
        string childEntityPlural, 
        string parentEntityName,
        string parentEntityPlural,
        string propertyName,
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
                        newText = @$"    public {parentEntityName} {propertyName} {{ get; }}{Environment.NewLine}{Environment.NewLine}{line}";
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
        var parentModelsClassPath = ClassPathHelper.EntityModelClassPath(srcDirectory, $"{property.ForeignEntityName}.cs", property.ForeignEntityPlural, EntityModel.Creation, projectBaseName);

        if (property.IsChildRelationship)
        {
            parentClassPath = ClassPathHelper.EntityClassPath(srcDirectory, $"{parentEntityName}.cs", parentEntityPlural, projectBaseName);
            classPath = ClassPathHelper.EntityClassPath(srcDirectory, $"{property.ForeignEntityName}.cs", property.ForeignEntityPlural, projectBaseName);
            parentModelsClassPath = ClassPathHelper.EntityModelClassPath(srcDirectory, $"{parentEntityName}.cs", parentEntityPlural, EntityModel.Creation, projectBaseName);
        }
        
        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
        {
            _consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
            return;
        }
        
        var parentUsingStatement = $@"using {parentClassPath.ClassNamespace};";
        if(property.GetDbRelationship.IsOneToOne)
            parentUsingStatement += $@"{Environment.NewLine}using {parentModelsClassPath.ClassNamespace};";
        
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

    public void AddStringArrayManagement(string srcDirectory, 
        EntityProperty property,
        string entityName,
        string entityPlural, 
        string projectBaseName)
    {
        var classPath = ClassPathHelper.EntityClassPath(srcDirectory, $"{entityName}.cs", entityPlural, projectBaseName);

        
        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
        {
            _consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
            return;
        }
        
        var arrayPropNameSingular = property.Name.Singularize().UppercaseFirstLetter();
        var arrayPropNameSingularLowerFirst = property.Name.Singularize().LowercaseFirstLetter();
        var arrayPropPlural = property.Name.UppercaseFirstLetter();
        var arrayPropPluralLowerFirst = property.Name.LowercaseFirstLetter();
        var managementText = $@"    public {entityName} Add{arrayPropNameSingular}(string {arrayPropNameSingularLowerFirst})
    {{
        {arrayPropPlural} ??= Array.Empty<string>();
        {arrayPropPlural} = {arrayPropPlural}.Append({arrayPropNameSingularLowerFirst}).ToArray();
        return this;
    }}

    public {entityName} Remove{arrayPropNameSingular}(string {arrayPropNameSingularLowerFirst})
    {{
        {arrayPropPlural} ??= Array.Empty<string>();
        {arrayPropPlural} = {arrayPropPlural}.Where(x => x != {arrayPropNameSingularLowerFirst}).ToArray();
        return this;
    }}

    public {entityName} Set{arrayPropPlural}(string[] {arrayPropPluralLowerFirst})
    {{
        {arrayPropPlural} = {arrayPropPluralLowerFirst};
        return this;
    }}";
        
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
                        newText = @$"{managementText}{Environment.NewLine}{Environment.NewLine}{line}";
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
        
        if (property.IsChildRelationship)
        {
            classPath = ClassPathHelper.EntityClassPath(srcDirectory, $"{property.ForeignEntityName}.cs", property.ForeignEntityPlural, projectBaseName);   
        }
        
        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
        {
            _consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
            return;
        }
        
        var managedListMethods = "";
        if (property.IsChildRelationship)
            managedListMethods += GetListManagementMethods(property.ForeignEntityName, parentEntityName, parentEntityPlural);
        else
            managedListMethods += GetListManagementMethods(parentEntityName, property.ForeignEntityName, property.ForeignEntityPlural);
        
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

        if (property.IsChildRelationship)
        {
            classPath = ClassPathHelper.EntityClassPath(srcDirectory, $"{property.ForeignEntityName}.cs", property.ForeignEntityPlural, projectBaseName);
        }
        
        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
        {
            _consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
            return;
        }
        
        var managedEntityMethod = "";
        if(property.IsChildRelationship)
            managedEntityMethod += GetEntityManagementMethods(property.ForeignEntityName, parentEntityName, property.Name);
        else
            managedEntityMethod += GetEntityManagementMethods(parentEntityName, property.ForeignEntityName, property.Name);
        
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
