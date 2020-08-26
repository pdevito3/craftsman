namespace Craftsman.Builders.Dtos
{
    using Craftsman.Enums;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class DtoFileTextGenerator
    {
        public static string GetPaginationDtoText(string classNamespace, Entity entity, Dto dto)
        {
            return @$"namespace {classNamespace}
{{
    public abstract class {Utilities.GetDtoName(entity.Name, dto)}
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
    public class {Utilities.GetDtoName(entity.Name, dto)} : {Utilities.GetDtoName(entity.Name, Dto.PaginationParamaters)}
    {{
        public string Filters {{ get; set; }}
        public string SortOrder {{ get; set; }}
    }}
}}";
        }

        public static string GetDtoText(ClassPath dtoClassPath, Entity entity, Dto dto)
        {
            var propString = dto == Dto.Creation || dto == Dto.Update ? "" : DtoPropBuilder(entity.Properties, dto);
            var manipulationString = dto == Dto.Creation || dto == Dto.Update ? $": {Utilities.GetDtoName(entity.Name, Dto.Manipulation)}" : "";
            var abstractString = dto == Dto.Manipulation ? $"abstract" : "";
            var fkUsingStatements = "";


            if (dto == Dto.Read)
            {
                foreach(var prop in entity.Properties.Where(p => p.IsForeignKey))
                {
                    fkUsingStatements += GetForeignKeyUsingStatements(dtoClassPath, fkUsingStatements, prop, dto);
                }
            }

            return @$"namespace {dtoClassPath.ClassNamespace}
{{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;{fkUsingStatements}

    public {abstractString} class {Utilities.GetDtoName(entity.Name, dto)} {manipulationString}
    {{
{propString}

        // add-on property marker - Do Not Delete This Comment
    }}
}}";
        }

        public static string GetForeignKeyUsingStatements(ClassPath dtoClassPath, string fkUsingStatements, EntityProperty prop, Dto dto)
        {
            var dtoFileName = $"{Utilities.GetDtoName(prop.Type, dto)}.cs";
            var fkClasspath = ClassPathHelper.DtoClassPath(dtoClassPath.SolutionDirectory, dtoFileName, prop.Type);

            fkUsingStatements += $"{Environment.NewLine}    using {fkClasspath.ClassNamespace};";   
            
            return fkUsingStatements;
        }

        public static string DtoPropBuilder(List<EntityProperty> props, Dto dto)
        {
            var propString = "";
            for(var eachProp = 0; eachProp < props.Count; eachProp++)
            {
                if (!props[eachProp].CanManipulate && dto == Dto.Manipulation)
                    continue;

                string newLine = eachProp == props.Count - 1 ? "" : Environment.NewLine;

                if (props[eachProp].IsForeignKey)
                {
                    if(dto == Dto.Read)
                    {
                        var dtoName = Utilities.GetDtoName(props[eachProp].Type, Dto.Read);
                        propString += $@"        public {dtoName} {props[eachProp].Name} {{ get; set; }}{newLine}";
                    }                    
                }
                else
                    propString += $@"        public {Utilities.PropTypeCleanup(props[eachProp].Type)} {props[eachProp].Name} {{ get; set; }}{newLine}";
            }

            return propString;
        }
    }
}
