namespace Craftsman.Builders.Endpoints
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using Domain;
    using Domain.Enums;
    using Services;

    public class ControllerModifier
    {
        private readonly IFileSystem _fileSystem;

        public ControllerModifier(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void AddEndpoint(string srcDirectory, FeatureType featureType, Entity entity, bool addSwaggerComments, Feature feature, string projectBaseName)
        {
            var classPath = ClassPathHelper.ControllerClassPath(srcDirectory, $"{FileNames.GetControllerName(entity.Plural)}.cs", projectBaseName, "v1");

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
                        if (line.Contains($"// endpoint marker"))
                        {
                            var endpoint = "";
                            if(featureType == FeatureType.GetList)
                                endpoint = GetListEndpointBuilder.GetEndpointTextForGetList(entity, addSwaggerComments, feature);
                            else if(featureType == FeatureType.GetRecord)
                                endpoint = GetRecordEndpointBuilder.GetEndpointTextForGetRecord(entity, addSwaggerComments, feature);
                            else if(featureType == FeatureType.AddRecord)
                                endpoint = CreateRecordEndpointBuilder.GetEndpointTextForCreateRecord(entity, addSwaggerComments, feature);
                            else if(featureType == FeatureType.AddListByFk)
                                endpoint = CreateRecordEndpointBuilder.GetEndpointTextForCreateList(entity, addSwaggerComments, feature);
                            else if(featureType == FeatureType.DeleteRecord)
                                endpoint = DeleteRecordEndpointBuilder.GetEndpointTextForDeleteRecord(entity, addSwaggerComments, feature);
                            else if(featureType == FeatureType.UpdateRecord)
                                endpoint = PutRecordEndpointBuilder.GetEndpointTextForPutRecord(entity, addSwaggerComments, feature);
                            else if(featureType == FeatureType.PatchRecord)
                                endpoint = PatchRecordEndpointBuilder.GetEndpointTextForPatchRecord(entity, addSwaggerComments, feature);

                            newText = $"{endpoint}{Environment.NewLine}{Environment.NewLine}{newText}";
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
}
