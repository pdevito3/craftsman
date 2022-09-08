namespace Craftsman.Builders;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Domain;
using Services;

public class DbContextModifier
{
    private readonly IFileSystem _fileSystem;

    public DbContextModifier(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public void AddDbSetAndConfig(string solutionDirectory, List<Entity> entities, string dbContextName, string projectBaseName)
    {
        var classPath = ClassPathHelper.DbContextClassPath(solutionDirectory, $"{dbContextName}.cs", projectBaseName);
        var entitiesUsings = "";
        foreach (var entity in entities)
        {
            var entityClassPath = ClassPathHelper.EntityClassPath(solutionDirectory, "", entity.Plural, projectBaseName);
            entitiesUsings += $"using {entityClassPath.ClassNamespace};{Environment.NewLine}"; // note this foreach adds newline after where dbbuilder adds before
        }

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
                    if (line.Contains($"#region DbSet Region"))
                    {
                        newText += @$"{Environment.NewLine}{DbContextBuilder.GetDbSetText(entities)}";
                    }

                    // TODO add test. assumes that this using exists and that the builder above adds a new line after the usings
                    if (line.Contains("using Microsoft.EntityFrameworkCore;"))
                    {
                        newText = $"{entitiesUsings}{line}";
                    }
                    
                    if (line.Contains($"#region Entity Database Config Region"))
                    {
                        newText += @$"{GetDbEntityConfigs(entities)}";
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        File.Delete(classPath.FullClassPath);
        File.Move(tempPath, classPath.FullClassPath);
    }


    
    public static string GetDbEntityConfigs(List<Entity> entities)
    {
        var configList = entities
            .Select(x => $"modelBuilder.ApplyConfiguration(new {FileNames.GetDatabaseEntityConfigName(x.Name)}());")
            .ToList();
        
        var newLinedString = configList.Aggregate((current, next) => @$"{current}{Environment.NewLine}        {next}");
        return newLinedString;
    }
}
