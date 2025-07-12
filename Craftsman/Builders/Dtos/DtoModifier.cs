namespace Craftsman.Builders.Dtos;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Domain;
using Domain.Enums;
using Services;
using MediatR;

public static class DtoModifier
{
    public sealed record AddPropertiesToDtosCommand(string EntityName, List<EntityProperty> Props, string ProjectBaseName) : IRequest;
    
    public class AddPropertiesToDtosHandler(IFileSystem fileSystem, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<AddPropertiesToDtosCommand>
    {
        public Task Handle(AddPropertiesToDtosCommand request, CancellationToken cancellationToken)
        {
            UpdateDtoFile(scaffoldingDirectoryStore.SolutionDirectory, request.EntityName, request.Props, Dto.Read, request.ProjectBaseName, fileSystem);
            UpdateDtoFile(scaffoldingDirectoryStore.SolutionDirectory, request.EntityName, request.Props, Dto.Creation, request.ProjectBaseName, fileSystem);
            UpdateDtoFile(scaffoldingDirectoryStore.SolutionDirectory, request.EntityName, request.Props, Dto.Update, request.ProjectBaseName, fileSystem);
            return Task.CompletedTask;
        }
    }

    private static void UpdateDtoFile(string solutionDirectory, string entityName, List<EntityProperty> props, Dto dto, string projectBaseName, IFileSystem fileSystem)
    {
        var dtoFileName = $"{FileNames.GetDtoName(entityName, dto)}.cs";
        var classPath = ClassPathHelper.DtoClassPath(solutionDirectory, dtoFileName, entityName, projectBaseName);

        if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
            throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

        if (!fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = fileSystem.File.CreateText(tempPath);
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
        fileSystem.File.Delete(classPath.FullClassPath);
        fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }
}
