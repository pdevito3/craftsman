namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text;

    public static class EntityBuilder
    {
        public static void CreateEntity(string solutionDirectory, Entity entity, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.EntityClassPath(solutionDirectory, $"{entity.Name}.cs", entity.Plural, projectBaseName);

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using var fs = fileSystem.File.Create(classPath.FullClassPath);
            var data = GetEntityFileText(classPath.ClassNamespace, entity);
            fs.Write(Encoding.UTF8.GetBytes(data));
        }

        public static string GetEntityFileText(string classNamespace, Entity entity)
        {
            var propString = EntityPropBuilder(entity.Properties);
            var usingSieve = entity.Properties.Where(e => e.CanFilter == true || e.CanSort == true).ToList().Count > 0 ? @$"{Environment.NewLine}    using Sieve.Attributes;" : "";
            var tableAnnotation = TableAnnotationBuilder(entity);

            return @$"namespace {classNamespace}
{{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;{usingSieve}

    {tableAnnotation}
    public class {entity.Name}
    {{
{propString}

        // add-on property marker - Do Not Delete This Comment
    }}
}}";
        }

        public static string TableAnnotationBuilder(Entity entity)
        {
            var tableName = entity.TableName ?? entity.Name;

            return entity.Schema != null ? @$"[Table(""{tableName}"", Schema=""{entity.Schema}"")]" : @$"[Table(""{tableName}"")]";
        }

        public static string EntityPropBuilder(List<EntityProperty> props)
        {
            var propString = "";
            for (var eachProp = 0; eachProp < props.Count; eachProp++)
            {
                var attributes = AttributeBuilder(props[eachProp]);
                propString += attributes;
                string defaultValue = Utilities.GetDefaultValueText(props[eachProp].DefaultValue, props[eachProp].Type);

                string newLine = eachProp == props.Count - 1 ? "" : $"{Environment.NewLine}{Environment.NewLine}";
                propString += $@"        public {props[eachProp].Type} {props[eachProp].Name} {{ get; set; }}{defaultValue}{newLine}";
            }

            return propString;
        }

        private static string AttributeBuilder(EntityProperty entityProperty)
        {
            var attributeString = "";

            if (entityProperty.IsPrimaryKey)
            {
                attributeString += @$"        [Key]{Environment.NewLine}";
            }
            if (entityProperty.IsRequired)
                attributeString += @$"        [Required]{Environment.NewLine}";
            if (entityProperty.IsForeignKey)
                attributeString += @$"        [ForeignKey(""{entityProperty.ForeignKeyPropName}"")]{Environment.NewLine}";
            if (entityProperty.CanFilter || entityProperty.CanSort)
                attributeString += @$"        [Sieve(CanFilter = {entityProperty.CanFilter.ToString().ToLower()}, CanSort = {entityProperty.CanSort.ToString().ToLower()})]{Environment.NewLine}";
            if (!string.IsNullOrEmpty(entityProperty.ColumnName))
                attributeString += @$"        [Column(""{entityProperty.ColumnName}"")]{Environment.NewLine}";

            return attributeString;
        }
    }
}