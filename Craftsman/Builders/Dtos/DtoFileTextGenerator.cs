namespace Craftsman.Builders.Dtos
{
    using Craftsman.Enums;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using Craftsman.Models.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class DtoFileTextGenerator
    {
        public static string GetReadParameterDtoText(string solutionDirectory, string classNamespace, Entity entity, Dto dto, string projectBaseName)
        {
            var sharedDtoClassPath = ClassPathHelper.SharedDtoClassPath(solutionDirectory, "", projectBaseName);

            return @$"namespace {classNamespace}
{{
    using {sharedDtoClassPath.ClassNamespace};

    public class {Utilities.GetDtoName(entity.Name, dto)} : BasePaginationParameters
    {{
        public string Filters {{ get; set; }}
        public string SortOrder {{ get; set; }}
    }}
}}";
        }

        public static string GetDtoText(IClassPath dtoClassPath, Entity entity, Dto dto, string projectBaseName)
        {
            var propString = DtoPropBuilder(entity.Properties, dto);
            if (dto == Dto.Creation)
            {
                propString = "";
                if (entity.PrimaryKeyProperty.Type.IsGuidPropertyType())
                    propString = DtoPropBuilder(new List<EntityProperty>() { entity.PrimaryKeyProperty }, dto);
            }
            else if (dto == Dto.Update)
                propString = "";

            var abstractString = dto == Dto.Manipulation ? $"abstract " : "";

            var inheritanceString = "";
            if (dto == Dto.Creation || dto == Dto.Update)
                inheritanceString = $": {Utilities.GetDtoName(entity.Name, Dto.Manipulation)}";

            return @$"namespace {dtoClassPath.ClassNamespace}
{{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public {abstractString}class {Utilities.GetDtoName(entity.Name, dto)} {inheritanceString}
    {{
{propString.RemoveLastNewLine()}

        // add-on property marker - Do Not Delete This Comment
    }}
}}";
        }

        public static string GetForeignKeyUsingStatements(ClassPath dtoClassPath, string fkUsingStatements, EntityProperty prop, Dto dto, string projectBaseName)
        {
            var dtoFileName = $"{Utilities.GetDtoName(prop.Type, dto)}.cs";
            var fkClasspath = ClassPathHelper.DtoClassPath(dtoClassPath.SolutionDirectory, dtoFileName, prop.Type, projectBaseName);

            fkUsingStatements += $"{Environment.NewLine}    using {fkClasspath.ClassNamespace};";

            return fkUsingStatements;
        }

        public static string DtoPropBuilder(List<EntityProperty> props, Dto dto)
        {
            var propString = "";
            for (var eachProp = 0; eachProp < props.Count; eachProp++)
            {
                if (!props[eachProp].CanManipulate && dto == Dto.Manipulation)
                    continue;
                var guidDefault = dto == Dto.Creation && props[eachProp].Type.IsGuidPropertyType()
                    ? " = Guid.NewGuid();"
                    : "";

                string newLine = eachProp == props.Count - 1 ? "" : Environment.NewLine;
                propString += $@"        public {props[eachProp].Type} {props[eachProp].Name} {{ get; set; }}{guidDefault}{newLine}";
            }

            return propString;
        }
    }
}