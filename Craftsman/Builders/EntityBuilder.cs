namespace Craftsman.Builders
{
    using System;
    using System.Collections.Generic;
    using Domain;
    using Domain.Enums;
    using Helpers;
    using Services;

    public class EntityBuilder
    {
        private readonly ICraftsmanUtilities _utilities;

        public EntityBuilder(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }
        
        public void CreateEntity(string solutionDirectory, string srcDirectory, Entity entity, string projectBaseName)
        {
            var classPath = ClassPathHelper.EntityClassPath(srcDirectory, $"{entity.Name}.cs", entity.Plural, projectBaseName);
            var fileText = GetEntityFileText(classPath.ClassNamespace, solutionDirectory, srcDirectory, entity, projectBaseName);
            _utilities.CreateFile(classPath, fileText);
        }

        public void CreateBaseEntity(string srcDirectory, string projectBaseName, bool useSoftDelete)
        {
            var classPath = ClassPathHelper.EntityClassPath(srcDirectory, $"BaseEntity.cs", "", projectBaseName);
            var fileText = GetBaseEntityFileText(classPath.ClassNamespace, useSoftDelete);
            _utilities.CreateFile(classPath, fileText);
        }

        public static string GetEntityFileText(string classNamespace, string solutionDirectory, string srcDirectory, Entity entity, string projectBaseName)
        {
            var creationDtoName = FileNames.GetDtoName(entity.Name, Dto.Creation);
            var creationValidatorName = FileNames.ValidatorNameGenerator(entity.Name, Validator.Creation);
            var updateDtoName = FileNames.GetDtoName(entity.Name, Dto.Update);
            var updateValidatorName = FileNames.ValidatorNameGenerator(entity.Name, Validator.Update);
            var profileName = FileNames.GetProfileName(entity.Name);
            var propString = EntityPropBuilder(entity.Properties);
            var usingSieve = entity.Properties.Where(e => e.CanFilter || e.CanSort).ToList().Count > 0 ? @$"{Environment.NewLine}using Sieve.Attributes;" : "";
            var tableAnnotation = EntityAnnotationBuilder(entity);

            var foreignEntityUsings = "";
            var foreignProps = entity.Properties.Where(e => e.IsForeignKey).ToList();
            foreach (var entityProperty in foreignProps)
            {
                var classPath = ClassPathHelper.EntityClassPath(srcDirectory, $"", entityProperty.ForeignEntityPlural, projectBaseName);

                foreignEntityUsings += $@"
using {classPath.ClassNamespace};";
            }

            var profileClassPath = ClassPathHelper.ProfileClassPath(srcDirectory, $"", entity.Plural, projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, $"", entity.Name, projectBaseName);
            var validatorClassPath = ClassPathHelper.ValidationClassPath(srcDirectory, $"", entity.Plural, projectBaseName);

            return @$"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using {profileClassPath.ClassNamespace};
using {validatorClassPath.ClassNamespace};
using AutoMapper;
using FluentValidation;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;{usingSieve}{foreignEntityUsings}

{tableAnnotation}
public class {entity.Name} : BaseEntity
{{
{propString}


    public static {entity.Name} Create({creationDtoName} {creationDtoName.LowercaseFirstLetter()})
    {{
        new {creationValidatorName}().ValidateAndThrow({creationDtoName.LowercaseFirstLetter()});
        var mapper = new Mapper(new MapperConfiguration(cfg => {{
            cfg.AddProfile<{profileName}>();
        }}));
        var new{entity.Name} = mapper.Map<{entity.Name}>({creationDtoName.LowercaseFirstLetter()});
        
        return new{entity.Name};
    }}
        
    public void Update({updateDtoName} {updateDtoName.LowercaseFirstLetter()})
    {{
        new {updateValidatorName}().ValidateAndThrow({updateDtoName.LowercaseFirstLetter()});
        var mapper = new Mapper(new MapperConfiguration(cfg => {{
            cfg.AddProfile<{profileName}>();
        }}));
        mapper.Map({updateDtoName.LowercaseFirstLetter()}, this);
    }}
    
    private {entity.Name}() {{ }} // For EF
}}";
        }

        public static string GetBaseEntityFileText(string classNamespace, bool useSoftDelete)
        {
            var isDeletedProp = useSoftDelete
                ? $@"
    public bool IsDeleted {{ get; private set; }}"
                : "";

            var isDeletedMethod = useSoftDelete
                ? $@"
    
    public void UpdateIsDeleted(bool isDeleted)
    {{
        IsDeleted = isDeleted;
    }}"
                : "";

            return @$"namespace {classNamespace};

using Sieve.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public abstract class BaseEntity
{{
    [Key]
    [Sieve(CanFilter = true, CanSort = true)]
    public Guid Id {{ get; private set; }} = Guid.NewGuid();
    public DateTime CreatedOn {{ get; private set; }}
    public string? CreatedBy {{ get; private set; }}
    public DateTime? LastModifiedOn {{ get; private set; }}
    public string? LastModifiedBy {{ get; private set; }}{isDeletedProp}

    public void UpdateCreationProperties(DateTime createdOn, string? createdBy)
    {{
        CreatedOn = createdOn;
        CreatedBy = createdBy;
    }}
    
    public void UpdateModifiedProperties(DateTime? lastModifiedOn, string? lastModifiedBy)
    {{
        LastModifiedOn = lastModifiedOn;
        LastModifiedBy = lastModifiedBy;
    }}{isDeletedMethod}
}}";
        }

        public static string EntityAnnotationBuilder(Entity entity)
        {
            if (string.IsNullOrEmpty(entity.TableName))
                return null;

            // must have table name to have a schema :-(
            var tableName = entity.TableName;
            return entity.Schema != null ? @$"[Table(""{tableName}"", Schema=""{entity.Schema}"")]" : @$"[Table(""{tableName}"")]";
        }

        public static string EntityPropBuilder(List<EntityProperty> props)
        {
            var propString = "";
            foreach (var property in props)
            {
                var attributes = AttributeBuilder(property);
                propString += attributes;
                var defaultValue = GetDefaultValueText(property.DefaultValue, property);
                var newLine = (property.IsForeignKey && !property.IsMany)
                    ? Environment.NewLine
                    : $"{Environment.NewLine}{Environment.NewLine}";

                if(property.IsPrimativeType || property.IsMany)
                    propString += $@"    public {property.Type} {property.Name} {{ get; private set; }}{defaultValue}{newLine}";

                propString += GetForeignProp(property);
            }

            return propString.RemoveLastNewLine().RemoveLastNewLine();
        }

        private static string GetDefaultValueText(string defaultValue, EntityProperty prop)
        {
            if (prop.Type == "string")
                return defaultValue == null ? "" : @$" = ""{defaultValue}"";";

            if ((prop.Type.IsGuidPropertyType() && !prop.Type.Contains("?") && !prop.IsForeignKey))
                return !string.IsNullOrEmpty(defaultValue) ? @$" = Guid.Parse(""{defaultValue}"");" : "";

            return string.IsNullOrEmpty(defaultValue) ? "" : $" = {defaultValue};";
        }
        
        private static string AttributeBuilder(EntityProperty entityProperty)
        {
            var attributeString = "";
            if (entityProperty.IsRequired)
                attributeString += @$"    [Required]{Environment.NewLine}";
            if (entityProperty.IsForeignKey
                && !entityProperty.IsMany
                && entityProperty.IsPrimativeType
            )
                attributeString += @$"    [JsonIgnore]
    [IgnoreDataMember]
    [ForeignKey(""{entityProperty.ForeignEntityName}"")]{Environment.NewLine}";
            if(entityProperty.IsMany || !entityProperty.IsPrimativeType)
                attributeString += $@"    [JsonIgnore]
    [IgnoreDataMember]{Environment.NewLine}";
            if (entityProperty.CanFilter || entityProperty.CanSort)
                attributeString += @$"    [Sieve(CanFilter = {entityProperty.CanFilter.ToString().ToLower()}, CanSort = {entityProperty.CanSort.ToString().ToLower()})]{Environment.NewLine}";

            attributeString += ColumnAttributeBuilder(entityProperty);

            return attributeString;
        }

        private static string ColumnAttributeBuilder(EntityProperty entityProperty)
        {
            if (!string.IsNullOrEmpty(entityProperty.ColumnName) || !string.IsNullOrEmpty(entityProperty.ColumnType))
            {
                var columnString = "";
                if (!string.IsNullOrEmpty(entityProperty.ColumnName))
                    columnString += @$"""{entityProperty.ColumnName}""";

                if (!string.IsNullOrEmpty(entityProperty.ColumnType))
                {
                    if (!string.IsNullOrEmpty(entityProperty.ColumnName))
                        columnString += ",";

                    columnString += @$"TypeName = ""{entityProperty.ColumnType}""";
                }

                return @$"    [Column({columnString})]{Environment.NewLine}";
            }

            return "";
        }

        private static string GetForeignProp(EntityProperty prop)
        {
            return !string.IsNullOrEmpty(prop.ForeignEntityName) && !prop.IsMany ? $@"    public {prop.ForeignEntityName} {prop.ForeignEntityName} {{ get; private set; }}{Environment.NewLine}{Environment.NewLine}" : "";
        }
    }
}
