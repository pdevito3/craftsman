namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Text;
    using static ConsoleWriter;

    public static class EntityDtoBuilder
    {
        public static void CreateEntity(string solutionDirectory, Entity entity)
        {
            try
            {
                //TODO move these to a dictionary to lookup and overwrite if I want
                var entityTopPath = "Domain\\Entities";
                var entityNamespace = entityTopPath.Replace("\\", ".");

                var entityDir = Path.Combine(solutionDirectory, entityTopPath);
                if (!Directory.Exists(entityDir))
                    Directory.CreateDirectory(entityDir);

                var pathString = Path.Combine(entityDir, $"{entity.Name.UppercaseFirstLetter()}.cs");
                if (File.Exists(pathString))
                    throw new FileAlreadyExistsException(pathString);

                using (FileStream fs = File.Create(pathString))
                {
                    var data = GetEntityFileText(entityNamespace, entity);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                WriteInfo($"A new '{entity.Name}' entity file was added here: {pathString}.");
            }
            catch (FileAlreadyExistsException)
            {
                WriteError("This file alread exists. Please enter a valid file path.");
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occured when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        public static void CreateDtos(string solutionDirectory, Entity entity)
        {
            try
            {
                //TODO move these to a dictionary to lookup and overwrite if I want
                var entityTopPath = $"Application\\Dtos\\{entity.Name}s";
                var entityNamespace = entityTopPath.Replace("\\", ".");

                var entityDir = Path.Combine(solutionDirectory, entityTopPath);
                if (!Directory.Exists(entityDir))
                    Directory.CreateDirectory(entityDir);

                CreateDtoFile(entityDir, entityNamespace, entity, Dto.Read);
                CreateDtoFile(entityDir, entityNamespace, entity, Dto.Manipulation);
                CreateDtoFile(entityDir, entityNamespace, entity, Dto.Creation);
                CreateDtoFile(entityDir, entityNamespace, entity, Dto.Update);
            }
            catch (FileAlreadyExistsException)
            {
                WriteError("This file alread exists. Please enter a valid file path.");
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occured when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        public static string GetEntityFileText(string classNamespace, Entity entity)
        {
            var propString = EntityPropBuilder(entity.Properties);
            var usingSieve = entity.Properties.Where(e => e.CanFilter == true || e.CanSort == true).ToList().Count > 0 ? $"    using Sieve.Attributes;{Environment.NewLine}" : "";
            return @$"namespace {classNamespace}
{{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
{usingSieve}
    public class {entity.Name}
    {{
{propString}
    }}
}}";
        }

        public static string GetDtoFileText(string classNamespace, Entity entity, Dto dto)
        {
            var propString = dto == Dto.Creation || dto == Dto.Update ? "" : DtoPropBuilder(entity.Properties, dto);
            var manipulationString = dto == Dto.Creation || dto == Dto.Update ? $": {DtoNameGenerator(entity.Name, Dto.Manipulation)}" : "";
            var abstractString = dto == Dto.Manipulation ? $"abstract" : "";

            return @$"namespace {classNamespace}
{{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public {abstractString} class {DtoNameGenerator(entity.Name, dto)} {manipulationString}
    {{
{propString}
    }}
}}";
        }

        public static void CreateDtoFile(string entityDir, string entityNamespace, Entity entity, Dto dto)
        {
            var pathString = Path.Combine(entityDir, $"{DtoNameGenerator(entity.Name.UppercaseFirstLetter(), dto)}.cs");
            if (File.Exists(pathString))
                throw new FileAlreadyExistsException(pathString);
            using (FileStream fs = File.Create(pathString))
            {
                var data = GetDtoFileText(entityNamespace, entity, dto);
                fs.Write(Encoding.UTF8.GetBytes(data));
                WriteInfo($"A new '{entity.Name}' update DTO file was added here: {pathString}.");
            }
        }

        private static string DtoNameGenerator(string entityName, Dto dto)
        {
            switch (dto)
            {
                case Dto.Manipulation:
                    return $"{entityName}DtoForManipulation";
                case Dto.Creation:
                    return $"{entityName}DtoForCreation";
                case Dto.Update:
                    return $"{entityName}DtoForUpdate";
                case Dto.Read:
                    return $"{entityName}Dto";
                default:
                    throw new Exception($"Name generator not configured for {Enum.GetName(typeof(Dto), dto)}");
            }
        }

        private static string EntityPropBuilder(List<EntityProperty> props)
        {
            var propString = "";
            for (var eachProp = 0; eachProp < props.Count; eachProp++)
            {
                var attributes = AttributeBuilder(props[eachProp]);
                propString += attributes;

                string newLine = eachProp == props.Count - 1 ? "" : $"{Environment.NewLine}{Environment.NewLine}";
                propString += $@"        public {PropTypeCleanup(props[eachProp].Type)} {props[eachProp].Name} {{ get; set; }}{newLine}";
            }

            return propString;
        }

        private static string AttributeBuilder(EntityProperty entityProperty)
        {
            var attributeString = "";

            if (entityProperty.IsPrimaryKey)
                attributeString += @$"        [Key]{Environment.NewLine}";
            if (entityProperty.IsRequired)
                attributeString += @$"        [Required]{Environment.NewLine}";
            if (entityProperty.CanFilter || entityProperty.CanSort)
                attributeString += @$"        [Sieve(CanFilter = {entityProperty.CanFilter}, CanSort = {entityProperty.CanSort})]{Environment.NewLine}";

            return attributeString;
        }

        private static string DtoPropBuilder(List<EntityProperty> props, Dto dto)
        {
            var propString = "";
            for (var eachProp = 0; eachProp < props.Count; eachProp++)
            {
                if (!props[eachProp].CanManipulate && dto == Dto.Manipulation)
                    continue;

                string newLine = eachProp == props.Count - 1 ? "" : Environment.NewLine;
                propString += $@"        public {PropTypeCleanup(props[eachProp].Type)} {props[eachProp].Name} {{ get; set; }}{newLine}";
            }

            return propString;
        }

        private static string PropTypeCleanup(string prop)
        {
            var lowercaseProps = new string[] { "string", "int", "decimal", "double", "float", "object", "bool", "byte", "char", "byte", "ushort", "uint", "ulong" };
            if (lowercaseProps.Contains(prop.ToLower()))
                return prop.ToLower();
            else if (prop.ToLower() == "datetime")
                return "DateTime";
            else if (prop.ToLower() == "datetime?")
                return "DateTime?";
            else
                return prop;
        }

    }
}
