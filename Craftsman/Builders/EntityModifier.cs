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
using MediatR;

public static class EntityModifier
{
    public sealed record AddSingularRelationshipEntityCommand(
        string ChildEntityName,
        string ChildEntityPlural,
        string ParentEntityName,
        string ParentEntityPlural,
        string PropertyName) : IRequest;

    public class AddSingularRelationshipEntityHandler(IFileSystem fileSystem, IConsoleWriter consoleWriter, IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<AddSingularRelationshipEntityCommand>
    {
        public Task Handle(AddSingularRelationshipEntityCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.EntityClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{request.ChildEntityName}.cs", request.ChildEntityPlural, scaffoldingDirectoryStore.ProjectBaseName);
            var parentClassPath = ClassPathHelper.EntityClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{request.ParentEntityName}.cs", request.ParentEntityPlural, scaffoldingDirectoryStore.ProjectBaseName);
            
            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (!fileSystem.File.Exists(classPath.FullClassPath))
            {
                consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
                return Task.CompletedTask;
            }
            
            var parentUsingStatement = $@"using {parentClassPath.ClassNamespace};";
            var usingStatementHasBeenAdded = false;
            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
            {
                using var output = fileSystem.File.CreateText(tempPath);
                {
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains($"Add Props Marker"))
                        {
                            newText = @$"    public {request.ParentEntityName} {request.PropertyName} {{ get; }}{Environment.NewLine}{Environment.NewLine}{line}";
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
            fileSystem.File.Delete(classPath.FullClassPath);
            fileSystem.File.Move(tempPath, classPath.FullClassPath);
            return Task.CompletedTask;
        }
    }

    public sealed record AddManyRelationshipEntityCommand(
        string ChildEntityName,
        string ChildEntityPlural,
        string ParentEntityName,
        string ParentEntityPlural) : IRequest;

    public class AddManyRelationshipEntityHandler(IFileSystem fileSystem, IConsoleWriter consoleWriter, IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<AddManyRelationshipEntityCommand>
    {
        public Task Handle(AddManyRelationshipEntityCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.EntityClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{request.ChildEntityName}.cs", request.ChildEntityPlural, scaffoldingDirectoryStore.ProjectBaseName);
            var parentClassPath = ClassPathHelper.EntityClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{request.ParentEntityName}.cs", request.ParentEntityPlural, scaffoldingDirectoryStore.ProjectBaseName);

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (!fileSystem.File.Exists(classPath.FullClassPath))
            {
                consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
                return Task.CompletedTask;
            }
            
            var parentUsingStatement = $@"using {parentClassPath.ClassNamespace};";
            var usingStatementHasBeenAdded = false;
            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
            {
                using var output = fileSystem.File.CreateText(tempPath);
                {
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains($"Add Props Marker"))
                        {
                            newText = @$"    public IReadOnlyCollection<{request.ParentEntityName}> {request.ParentEntityPlural} {{ get; }} = new List<{request.ParentEntityName}>();{Environment.NewLine}{Environment.NewLine}{line}";
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
            fileSystem.File.Delete(classPath.FullClassPath);
            fileSystem.File.Move(tempPath, classPath.FullClassPath);
            return Task.CompletedTask;
        }
    }

    public sealed record AddParentRelationshipEntityCommand(
        EntityProperty Property,
        string ParentEntityName,
        string ParentEntityPlural) : IRequest;

    public class AddParentRelationshipEntityHandler(IFileSystem fileSystem, IConsoleWriter consoleWriter, IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<AddParentRelationshipEntityCommand>
    {
        public Task Handle(AddParentRelationshipEntityCommand request, CancellationToken cancellationToken)
        {
            var property = request.Property;
            var parentClassPath = ClassPathHelper.EntityClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{property.ForeignEntityName}.cs", property.ForeignEntityPlural, scaffoldingDirectoryStore.ProjectBaseName);
            var classPath = ClassPathHelper.EntityClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{request.ParentEntityName}.cs", request.ParentEntityPlural, scaffoldingDirectoryStore.ProjectBaseName);
            var parentModelsClassPath = ClassPathHelper.EntityModelClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{property.ForeignEntityName}.cs", property.ForeignEntityPlural, EntityModel.Creation, scaffoldingDirectoryStore.ProjectBaseName);

            if (property.IsChildRelationship)
            {
                parentClassPath = ClassPathHelper.EntityClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{request.ParentEntityName}.cs", request.ParentEntityPlural, scaffoldingDirectoryStore.ProjectBaseName);
                classPath = ClassPathHelper.EntityClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{property.ForeignEntityName}.cs", property.ForeignEntityPlural, scaffoldingDirectoryStore.ProjectBaseName);
                parentModelsClassPath = ClassPathHelper.EntityModelClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{request.ParentEntityName}.cs", request.ParentEntityPlural, EntityModel.Creation, scaffoldingDirectoryStore.ProjectBaseName);
            }
            
            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (!fileSystem.File.Exists(classPath.FullClassPath))
            {
                consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
                return Task.CompletedTask;
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
                request.ParentEntityName,
                request.ParentEntityPlural);
            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
            {
                using var output = fileSystem.File.CreateText(tempPath);
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
            fileSystem.File.Delete(classPath.FullClassPath);
            fileSystem.File.Move(tempPath, classPath.FullClassPath);
            return Task.CompletedTask;
        }
    }

    public sealed record AddStringArrayManagementCommand(
        EntityProperty Property,
        string EntityName,
        string EntityPlural) : IRequest;

    public class AddStringArrayManagementHandler(IFileSystem fileSystem, IConsoleWriter consoleWriter, IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<AddStringArrayManagementCommand>
    {
        public Task Handle(AddStringArrayManagementCommand request, CancellationToken cancellationToken)
        {
            var property = request.Property;
            var classPath = ClassPathHelper.EntityClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{request.EntityName}.cs", request.EntityPlural, scaffoldingDirectoryStore.ProjectBaseName);

            
            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (!fileSystem.File.Exists(classPath.FullClassPath))
            {
                consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
                return Task.CompletedTask;
            }
            
            var arrayPropNameSingular = property.Name.Singularize().UppercaseFirstLetter();
            var arrayPropNameSingularLowerFirst = property.Name.Singularize().LowercaseFirstLetter();
            var arrayPropPlural = property.Name.UppercaseFirstLetter();
            var arrayPropPluralLowerFirst = property.Name.LowercaseFirstLetter();
            var managementText = $@"    public {request.EntityName} Add{arrayPropNameSingular}(string {arrayPropNameSingularLowerFirst})
    {{
        {arrayPropPlural} ??= Array.Empty<string>();
        {arrayPropPlural} = {arrayPropPlural}.Append({arrayPropNameSingularLowerFirst}).ToArray();
        return this;
    }}

    public {request.EntityName} Remove{arrayPropNameSingular}(string {arrayPropNameSingularLowerFirst})
    {{
        {arrayPropPlural} ??= Array.Empty<string>();
        {arrayPropPlural} = {arrayPropPlural}.Where(x => x != {arrayPropNameSingularLowerFirst}).ToArray();
        return this;
    }}

    public {request.EntityName} Set{arrayPropPlural}(string[] {arrayPropPluralLowerFirst})
    {{
        {arrayPropPlural} = {arrayPropPluralLowerFirst};
        return this;
    }}";
            
            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
            {
                using var output = fileSystem.File.CreateText(tempPath);
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
            fileSystem.File.Delete(classPath.FullClassPath);
            fileSystem.File.Move(tempPath, classPath.FullClassPath);
            return Task.CompletedTask;
        }
    }

    public sealed record AddEntityManyManagementMethodsCommand(
        EntityProperty Property,
        string ParentEntityName,
        string ParentEntityPlural) : IRequest;

    public class AddEntityManyManagementMethodsHandler(IFileSystem fileSystem, IConsoleWriter consoleWriter, IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<AddEntityManyManagementMethodsCommand>
    {
        public Task Handle(AddEntityManyManagementMethodsCommand request, CancellationToken cancellationToken)
        {
            var property = request.Property;
            var classPath = ClassPathHelper.EntityClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{request.ParentEntityName}.cs", request.ParentEntityPlural, scaffoldingDirectoryStore.ProjectBaseName);
            
            if (property.IsChildRelationship)
            {
                classPath = ClassPathHelper.EntityClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{property.ForeignEntityName}.cs", property.ForeignEntityPlural, scaffoldingDirectoryStore.ProjectBaseName);   
            }
            
            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (!fileSystem.File.Exists(classPath.FullClassPath))
            {
                consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
                return Task.CompletedTask;
            }
            
            var managedListMethods = "";
            if (property.IsChildRelationship)
                managedListMethods += GetListManagementMethods(property.ForeignEntityName, request.ParentEntityName, request.ParentEntityPlural);
            else
                managedListMethods += GetListManagementMethods(request.ParentEntityName, property.ForeignEntityName, property.ForeignEntityPlural);
            
            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
            {
                using var output = fileSystem.File.CreateText(tempPath);
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
            fileSystem.File.Delete(classPath.FullClassPath);
            fileSystem.File.Move(tempPath, classPath.FullClassPath);
            return Task.CompletedTask;
        }
    }

    public sealed record AddEntitySingularManagementMethodsCommand(
        EntityProperty Property,
        string ParentEntityName,
        string ParentEntityPlural) : IRequest;

    public class AddEntitySingularManagementMethodsHandler(IFileSystem fileSystem, IConsoleWriter consoleWriter, IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<AddEntitySingularManagementMethodsCommand>
    {
        public Task Handle(AddEntitySingularManagementMethodsCommand request, CancellationToken cancellationToken)
        {
            var property = request.Property;
            var classPath = ClassPathHelper.EntityClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{request.ParentEntityName}.cs", request.ParentEntityPlural, scaffoldingDirectoryStore.ProjectBaseName);

            if (property.IsChildRelationship)
            {
                classPath = ClassPathHelper.EntityClassPath(scaffoldingDirectoryStore.SrcDirectory, $"{property.ForeignEntityName}.cs", property.ForeignEntityPlural, scaffoldingDirectoryStore.ProjectBaseName);
            }
            
            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (!fileSystem.File.Exists(classPath.FullClassPath))
            {
                consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
                return Task.CompletedTask;
            }
            
            var managedEntityMethod = "";
            if(property.IsChildRelationship)
                managedEntityMethod += GetEntityManagementMethods(property.ForeignEntityName, request.ParentEntityName, property.Name);
            else
                managedEntityMethod += GetEntityManagementMethods(request.ParentEntityName, property.ForeignEntityName, property.Name);
            
            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
            {
                using var output = fileSystem.File.CreateText(tempPath);
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
            fileSystem.File.Delete(classPath.FullClassPath);
            fileSystem.File.Move(tempPath, classPath.FullClassPath);
            return Task.CompletedTask;
        }
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