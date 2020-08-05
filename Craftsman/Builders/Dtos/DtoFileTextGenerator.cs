namespace Craftsman.Builders.Dtos
{
    using Craftsman.Enums;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;

    public static class DtoFileTextGenerator
    {
        public static string GetPaginationDtoText(string classNamespace, Entity entity, Dto dto)
        {
            return @$"namespace {classNamespace}
{{
    public abstract class {Utilities.DtoNameGenerator(entity.Name, dto)}
    {{
        const int maxPageSize = 20;
        public int PageNumber {{ get; set; }} = 1;

        private int _pageSize = 10;
        public int PageSize
        {{
            get
            {{
                return _pageSize;
            }}
            set
            {{
                _pageSize = value > maxPageSize ? maxPageSize : value;
            }}
        }}
    }}
}}";
        }

        public static string GetReadParameterDtoText(string classNamespace, Entity entity, Dto dto)
        {
            return @$"namespace {classNamespace}
{{
    public class {Utilities.DtoNameGenerator(entity.Name, dto)} : {Utilities.DtoNameGenerator(entity.Name, Dto.PaginationParamaters)}
    {{
        public string Filters {{ get; set; }}
        public string SortOrder {{ get; set; }}
    }}
}}";
        }

        public static string GetDtoText(string classNamespace, Entity entity, Dto dto)
        {
            var propString = dto == Dto.Creation || dto == Dto.Update ? "" : DtoPropBuilder(entity.Properties, dto);
            var manipulationString = dto == Dto.Creation || dto == Dto.Update ? $": {Utilities.DtoNameGenerator(entity.Name, Dto.Manipulation)}" : "";
            var abstractString = dto == Dto.Manipulation ? $"abstract" : "";

            return @$"namespace {classNamespace}
{{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public {abstractString} class {Utilities.DtoNameGenerator(entity.Name, dto)} {manipulationString}
    {{
{propString}
    }}
}}";
        }

        public static string DtoPropBuilder(List<EntityProperty> props, Dto dto)
        {
            var propString = "";
            for (var eachProp = 0; eachProp < props.Count; eachProp++)
            {
                if (!props[eachProp].CanManipulate && dto == Dto.Manipulation)
                    continue;

                string newLine = eachProp == props.Count - 1 ? "" : Environment.NewLine;
                propString += $@"        public {Utilities.PropTypeCleanup(props[eachProp].Type)} {props[eachProp].Name} {{ get; set; }}{newLine}";
            }

            return propString;
        }
    }
}
