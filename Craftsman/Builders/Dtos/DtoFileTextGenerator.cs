namespace Craftsman.Builders.Dtos;

using System;
using System.Collections.Generic;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public static class DtoFileTextGenerator
{
    public static string GetReadParameterDtoText(string srcDirectory, string classNamespace, Entity entity, Dto dto, string projectBaseName)
    {
        var webApiResourcesClassPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "", projectBaseName);

        return @$"namespace {classNamespace};

using {webApiResourcesClassPath.ClassNamespace};

public sealed class {FileNames.GetDtoName(entity.Name, dto)} : BasePaginationParameters
{{
    public string? Filters {{ get; set; }}
    public string? SortOrder {{ get; set; }}
}}
";
    }

    public static string GetDtoText(IClassPath dtoClassPath, Entity entity, Dto dto)
    {
        var propString = dto is Dto.Read ? $"    public Guid Id {{ get; set; }}{Environment.NewLine}" : "";
        propString += DtoPropBuilder(entity.Properties, dto);

        return @$"namespace {dtoClassPath.ClassNamespace};

using Destructurama.Attributed;

public sealed record {FileNames.GetDtoName(entity.Name, dto)}
{{
{propString}
}}
";
    }

    public static string DtoPropBuilder(List<EntityProperty> props, Dto dto)
    {
        var propString = "";
        for (var eachProp = 0; eachProp < props.Count; eachProp++)
        {
            if(!props[eachProp].GetDbRelationship.IsNone)
                continue;
            if (!props[eachProp].CanManipulate && (dto is Dto.Creation or Dto.Update))
                continue;
            if (!props[eachProp].IsPrimitiveType && !props[eachProp].IsStringArray && !props[eachProp].IsValueObject)
                continue;

            var defaultValue = props[eachProp].IsStringArray ? " = Array.Empty<string>();" : "";

            string summary = string.IsNullOrWhiteSpace(props[eachProp].Summary) ? "" : $@"    ///<summary>{Environment.NewLine}    ///{props[eachProp].Summary}{Environment.NewLine}    ///</summary>{Environment.NewLine}";
            string example = string.IsNullOrWhiteSpace(props[eachProp].Example) ? "" : $@"    ///<example>{props[eachProp].Example}</example>{Environment.NewLine}";


            var attributes = AttributeBuilder(props[eachProp]);
            string newLine = eachProp == props.Count - 1 ? "" : Environment.NewLine;

            propString += $@"{Environment.NewLine}{summary}{example}{attributes}    public {props[eachProp].Type} {props[eachProp].Name} {{ get; set; }}{defaultValue}{newLine}";

        }

        return propString;
    }

    private static string AttributeBuilder(EntityProperty entityProperty)
    {
        var attributeString = "";
        
        if(entityProperty.IsLogMasked)
            attributeString += $@"    [LogMasked]{Environment.NewLine}";

        return attributeString;
    }
}
