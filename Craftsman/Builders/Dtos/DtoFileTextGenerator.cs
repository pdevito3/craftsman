namespace Craftsman.Builders.Dtos;

using System;
using System.Collections.Generic;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public static class DtoFileTextGenerator
{
    public static string GetReadParameterDtoText(string solutionDirectory, string classNamespace, Entity entity, Dto dto)
    {
        var sharedDtoClassPath = ClassPathHelper.SharedDtoClassPath(solutionDirectory, "");

        return @$"namespace {classNamespace};

using {sharedDtoClassPath.ClassNamespace};

public sealed record {FileNames.GetDtoName(entity.Name, dto)} : BasePaginationParameters
{{
    public string Filters {{ get; set; }}
    public string SortOrder {{ get; set; }}
}}
";
    }

    public static string GetDtoText(IClassPath dtoClassPath, Entity entity, Dto dto)
    {
        var propString = dto is Dto.Read ? $"    public Guid Id {{ get; set; }}{Environment.NewLine}" : "";
        propString += DtoPropBuilder(entity.Properties, dto);

        return @$"namespace {dtoClassPath.ClassNamespace};

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
            if (!props[eachProp].CanManipulate && (dto is Dto.Creation or Dto.Update))
                continue;
            if (props[eachProp].IsForeignKey && props[eachProp].IsMany)
                continue;
            if (!props[eachProp].IsPrimitiveType)
                continue;

            string newLine = eachProp == props.Count - 1 ? "" : Environment.NewLine;
            propString += $@"    public {props[eachProp].Type} {props[eachProp].Name} {{ get; set; }}{newLine}";
        }

        return propString;
    }
}
