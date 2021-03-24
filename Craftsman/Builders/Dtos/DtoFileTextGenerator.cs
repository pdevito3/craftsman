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

        public static string GetDtoText(ClassPath dtoClassPath, Entity entity, Dto dto, string projectBaseName)
        {
            var propString = dto == Dto.Creation || dto == Dto.Update ? "" : DtoPropBuilder(entity.Properties, dto);
            var fkUsingStatements = "";
            var abstractString = dto == Dto.Manipulation ? $"abstract" : "";

            var inheritanceString = "";
            if(dto == Dto.Creation || dto == Dto.Update)
                inheritanceString = $": {Utilities.GetDtoName(entity.Name, Dto.Manipulation)}";

            if (dto == Dto.Read)
            {
                foreach(var prop in entity.Properties.Where(p => p.IsForeignKey))
                {
                    fkUsingStatements += GetForeignKeyUsingStatements(dtoClassPath, fkUsingStatements, prop, dto, projectBaseName);
                }
            }

            return @$"namespace {dtoClassPath.ClassNamespace}
{{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;{fkUsingStatements}

    public {abstractString} class {Utilities.GetDtoName(entity.Name, dto)} {inheritanceString}
    {{
{propString}

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
                    propString += $@"        public {props[eachProp].Type} {props[eachProp].Name} {{ get; set; }}{newLine}";
            }

            return propString;
        }
    }
}
