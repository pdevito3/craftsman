namespace Craftsman.Builders
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Endpoints;
    using Enums;

    public class ControllerModifier
    {
        public static void AddEndpoint(string srcDirectory, FeatureType featureType, Entity entity, bool addSwaggerComments, Feature feature, string projectBaseName)
        {
            var classPath = ClassPathHelper.ControllerClassPath(srcDirectory, $"{Utilities.GetControllerName(entity.Plural)}.cs", projectBaseName, "v1");

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using (var output = new StreamWriter(tempPath))
                {
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains($"// endpoint marker"))
                        {
                            var endpoint = "";
                            if(featureType == FeatureType.GetList)
                                endpoint = GetListEndpointBuilder.GetEndpointTextForGetList(entity, addSwaggerComments, feature.PermissionName);
                            else if(featureType == FeatureType.GetRecord)
                                endpoint = GetRecordEndpointBuilder.GetEndpointTextForGetRecord(entity, addSwaggerComments, feature.PermissionName);
                            else if(featureType == FeatureType.AddRecord)
                                endpoint = CreateRecordEndpointBuilder.GetEndpointTextForCreateRecord(entity, addSwaggerComments, feature.PermissionName);
                            else if(featureType == FeatureType.AddListByFk)
                                endpoint = CreateRecordEndpointBuilder.GetEndpointTextForCreateList(entity, addSwaggerComments, feature);
                            else if(featureType == FeatureType.DeleteRecord)
                                endpoint = DeleteRecordEndpointBuilder.GetEndpointTextForDeleteRecord(entity, addSwaggerComments, feature.PermissionName);
                            else if(featureType == FeatureType.UpdateRecord)
                                endpoint = PutRecordEndpointBuilder.GetEndpointTextForPutRecord(entity, addSwaggerComments, feature.PermissionName);
                            else if(featureType == FeatureType.PatchRecord)
                                endpoint = PatchRecordEndpointBuilder.GetEndpointTextForPatchRecord(entity, addSwaggerComments, feature.PermissionName);

                            newText = $"{endpoint}{Environment.NewLine}{Environment.NewLine}{newText}";
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);
        }
    }
}
