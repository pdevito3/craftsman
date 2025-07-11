namespace Craftsman.Builders.Endpoints;

using System;
using System.IO;
using System.IO.Abstractions;
using Domain;
using Domain.Enums;
using Services;
using MediatR;

public static class ControllerModifier
{
    public sealed record AddEndpointCommand(FeatureType FeatureType, Entity Entity, bool AddSwaggerComments, Feature Feature) : IRequest;
    public sealed record AddCustomUserEndpointCommand() : IRequest;

    public class AddEndpointHandler(IFileSystem fileSystem, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<AddEndpointCommand>
    {
        public Task Handle(AddEndpointCommand request, CancellationToken cancellationToken)
        {
            AddEndpoint(scaffoldingDirectoryStore.SrcDirectory, request.FeatureType, request.Entity, request.AddSwaggerComments, request.Feature, scaffoldingDirectoryStore.ProjectBaseName, fileSystem);
            return Task.CompletedTask;
        }
    }

    public class AddCustomUserEndpointHandler(IFileSystem fileSystem, IScaffoldingDirectoryStore scaffoldingDirectoryStore) : IRequestHandler<AddCustomUserEndpointCommand>
    {
        public Task Handle(AddCustomUserEndpointCommand request, CancellationToken cancellationToken)
        {
            AddCustomUserEndpoint(scaffoldingDirectoryStore.SrcDirectory, scaffoldingDirectoryStore.ProjectBaseName, fileSystem);
            return Task.CompletedTask;
        }
    }

    private static void AddEndpoint(string srcDirectory, FeatureType featureType, Entity entity, bool addSwaggerComments, Feature feature, string projectBaseName, IFileSystem fileSystem)
    {
        var classPath = ClassPathHelper.ControllerClassPath(srcDirectory, $"{FileNames.GetControllerName(entity.Plural)}.cs", projectBaseName, "v1");

        if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
            fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

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
                    if (line.Contains($"// endpoint marker"))
                    {
                        var endpoint = "";
                        if (featureType == FeatureType.GetList)
                            endpoint = GetListEndpointBuilder.GetEndpointTextForGetList(entity, addSwaggerComments, feature);
                        else if (featureType == FeatureType.GetRecord)
                            endpoint = GetRecordEndpointBuilder.GetEndpointTextForGetRecord(entity, addSwaggerComments, feature);
                        else if (featureType == FeatureType.GetAll)
                            endpoint = GetAllEndpointBuilder.GetEndpointTextForGetAll(entity, addSwaggerComments, feature);
                        else if (featureType == FeatureType.AddRecord)
                            endpoint = CreateRecordEndpointBuilder.GetEndpointTextForCreateRecord(entity, addSwaggerComments, feature);
                        else if (featureType == FeatureType.AddListByFk)
                            endpoint = CreateRecordEndpointBuilder.GetEndpointTextForCreateList(entity, addSwaggerComments, feature);
                        else if (featureType == FeatureType.DeleteRecord)
                            endpoint = DeleteRecordEndpointBuilder.GetEndpointTextForDeleteRecord(entity, addSwaggerComments, feature);
                        else if (featureType == FeatureType.UpdateRecord)
                            endpoint = PutRecordEndpointBuilder.GetEndpointTextForPutRecord(entity, addSwaggerComments, feature);
                        // else if (featureType == FeatureType.PatchRecord)
                        //     endpoint = PatchRecordEndpointBuilder.GetEndpointTextForPatchRecord(entity, addSwaggerComments, feature);

                        newText = $"{endpoint}{Environment.NewLine}{Environment.NewLine}{newText}";
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        fileSystem.File.Delete(classPath.FullClassPath);
        fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }
    private static void AddCustomUserEndpoint(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
    {
        var classPath = ClassPathHelper.ControllerClassPath(srcDirectory, $"{FileNames.GetControllerName("Users")}.cs", projectBaseName, "v1");

        if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
            fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

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
                    if (line.Contains($"// endpoint marker"))
                    {
                        var endpoint = "";
                        endpoint += CustomUserEndpointsBuilder.AddRoleEndpoint();
                        endpoint += $"{Environment.NewLine}{Environment.NewLine}{CustomUserEndpointsBuilder.RemoveRoleEndpoint()}";
                        newText = $"{endpoint}{Environment.NewLine}{Environment.NewLine}{newText}";
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
