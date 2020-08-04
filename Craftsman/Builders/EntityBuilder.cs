namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public static class EntityBuilder
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

        private static string EntityPropBuilder(List<EntityProperty> props)
        {
            var propString = "";
            for (var eachProp = 0; eachProp < props.Count; eachProp++)
            {
                var attributes = AttributeBuilder(props[eachProp]);
                propString += attributes;

                string newLine = eachProp == props.Count - 1 ? "" : $"{Environment.NewLine}{Environment.NewLine}";
                propString += $@"        public {Utilities.PropTypeCleanup(props[eachProp].Type)} {props[eachProp].Name} {{ get; set; }}{newLine}";
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
    }
}
