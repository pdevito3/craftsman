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
        public static void CreateEntity(string srcDirectory, Entity entity, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.EntityClassPath(srcDirectory, $"{entity.Name}.cs", entity.Plural, projectBaseName);
            var fileText = GetEntityFileText(classPath.ClassNamespace, entity);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        public static void CreateBaseEntity(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.EntityClassPath(srcDirectory, $"BaseEntity.cs", "", projectBaseName);
            var fileText = GetBaseEntityFileText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static string GetEntityFileText(string classNamespace, Entity entity)
        {
            var propString = EntityPropBuilder(entity.Properties);
            var usingSieve = entity.Properties.Where(e => e.CanFilter || e.CanSort).ToList().Count > 0 ? @$"{Environment.NewLine}    using Sieve.Attributes;" : "";
            var tableAnnotation = TableAnnotationBuilder(entity);

            return @$"namespace {classNamespace}
{{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;{usingSieve}

    {tableAnnotation}
    public class {entity.Name} : BaseEntity
    {{
{propString}
    }}
}}";
        }

        public static string GetBaseEntityFileText(string classNamespace)
        {
            return @$"namespace {classNamespace}
{{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public abstract class BaseEntity
    {{
        [Key] 
        [Column(""id"")]
        public Guid Id {{ get; set; }} = Guid.NewGuid();
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
            foreach (var property in props)
            {
                var attributes = AttributeBuilder(property);
                propString += attributes;
                var defaultValue = Utilities.GetDefaultValueText(property.DefaultValue, property.Type);
                var newLine = property == props.LastOrDefault() ? "" : $"{Environment.NewLine}{Environment.NewLine}";
                propString += $@"        public {property.Type} {property.Name} {{ get; set; }}{defaultValue}{newLine}";
            }

            return propString;
        }

        private static string AttributeBuilder(EntityProperty entityProperty)
        {
            var attributeString = "";
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