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
                var classPath = ClassPathHelper.EntityClassPath(solutionDirectory, $"{entity.Name}.cs");

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = GetEntityFileText(classPath.ClassNamespace, entity);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}\\", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
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

        // add-on property marker - Do Not Delete This Comment
    }}
}}";
        }

        public static string EntityPropBuilder(List<EntityProperty> props)
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
                attributeString += @$"        [Sieve(CanFilter = {entityProperty.CanFilter.ToString().ToLower()}, CanSort = {entityProperty.CanSort.ToString().ToLower()})]{Environment.NewLine}";

            return attributeString;
        }
    }
}
