namespace Craftsman.Domain;

using Ardalis.SmartEnum;
using Builders;
using Enums;
using Helpers;
using Services;

public abstract class DbRelationship : SmartEnum<DbRelationship>
{
    public static readonly DbRelationship None = new NoneType();
    public static readonly DbRelationship OneToMany = new OneToManyType();
    public static readonly DbRelationship ManyToOne = new ManyToOneType();
    public static readonly DbRelationship OneToOne = new OneToOneType();
    public static readonly DbRelationship ManyToMany = new ManyToManyType();
    public static readonly DbRelationship Self = new SelfType();
    
    public static DbRelationship NoRelationship(bool isChildRelationship = false) => new NoneType(isChildRelationship);
    public static DbRelationship OneToManyRelationship(bool isChildRelationship = false) => new OneToManyType(isChildRelationship);
    public static DbRelationship ManyToOneRelationship(bool isChildRelationship = false) => new ManyToOneType(isChildRelationship);
    public static DbRelationship OneToOneRelationship(bool isChildRelationship = false) => new OneToOneType(isChildRelationship);
    public static DbRelationship ManyToManyRelationship(bool isChildRelationship = false) => new ManyToManyType(isChildRelationship);
    public static DbRelationship SelfRelationship(bool isChildRelationship = false) => new SelfType(isChildRelationship);


    public bool IsNone => this == None;
    public bool IsOneToMany => this == OneToMany;
    public bool IsManyToOne => this == ManyToOne;
    public bool IsOneToOne => this == OneToOne;
    public bool IsManyToMany => this == ManyToMany;
    public bool IsSelf => this == Self;
    

    public bool IsChildRelationship { get; protected set; }
    public void SetChildRelationship(bool isChildRelationship) => IsChildRelationship = isChildRelationship;
    protected DbRelationship(string name, int value, bool isChildRelationship = false) : base(name, value)
    {
        IsChildRelationship = isChildRelationship;
    }

    public abstract string GetPrincipalPropString(string propertyType, string propertyName, string defaultValue, string foreignEntityName, string foreignEntityPlural, string entityName, string entityPlural);
    public abstract string GetEntityDbConfig(string entityName, string entityPlural, string propertyName, string foreignEntityPlural, string foreignEntityName);
    public abstract void UpdateEntityProperties(EntityModifier entityModifier, string srcDirectory, string entityName,
        string entityPlural, string foreignEntityName, string foreignEntityPlural, string propertyName, string projectBaseName);
    public abstract void UpdateEntityManagementMethods(EntityModifier entityModifier, string srcDirectory, string entityName,
        string entityPlural, EntityProperty entityProperty, string projectBaseName);

    private class NoneType : DbRelationship
    {
        public NoneType(bool isChildRelationship = false) : base("none", 0, isChildRelationship) { }

        public override string GetEntityDbConfig(string entityName, string entityPlural, string propertyName, string foreignEntityPlural, string foreignEntityName)
            => null;

        public override void UpdateEntityProperties(EntityModifier entityModifier, string srcDirectory, string entityName,
            string entityPlural, string foreignEntityName, string foreignEntityPlural, string propertyName, string projectBaseName)
        {
            // no op
        }
        
        public override void UpdateEntityManagementMethods(EntityModifier entityModifier, string srcDirectory, string entityName,
            string entityPlural, EntityProperty entityProperty, string projectBaseName)
        {
            // no op
        }

        public override string GetPrincipalPropString(string propertyType, string propertyName, string defaultValue, string foreignEntityName, string foreignEntityPlural, string entityName, string entityPlural) 
            => $@"    public {propertyType} {propertyName} {{ get; private set; }}{defaultValue}{Environment.NewLine}{Environment.NewLine}";
    }

    private class OneToManyType : DbRelationship
    {
        public OneToManyType(bool isChildRelationship = false) : base("1tomany", 1, isChildRelationship) { }

        public override string GetEntityDbConfig(string entityName, string entityPlural, string propertyName,
            string foreignEntityPlural, string foreignEntityName)
        {
            if (IsChildRelationship)
                return @$"{Environment.NewLine}        builder.HasOne(x => x.{propertyName})
            .WithMany(x => x.{foreignEntityPlural});";
            
            return @$"{Environment.NewLine}        builder.HasMany(x => x.{foreignEntityPlural})
            .WithOne(x => x.{entityName});";
        }

        public override string GetPrincipalPropString(string propertyType, string propertyName, string defaultValue, string foreignEntityName, string foreignEntityPlural, string entityName, string entityPlural)
        {
            if (IsChildRelationship)
            {
                return $@"    public {entityName} {propertyName} {{ get; private set; }}{Environment.NewLine}";
            }
            
            var lowerPropName = foreignEntityPlural.LowercaseFirstLetter();
            return $@"    private readonly List<{foreignEntityName}> _{lowerPropName} = new();
    public IReadOnlyCollection<{foreignEntityName}> {foreignEntityPlural} => _{lowerPropName}.AsReadOnly();{Environment.NewLine}";
        }

        public override void UpdateEntityProperties(EntityModifier entityModifier, string srcDirectory, string entityName,
            string entityPlural, string foreignEntityName, string foreignEntityPlural, string propertyName, string projectBaseName)
        {
            if (IsChildRelationship)
            {
                entityModifier.AddManyRelationshipEntity(srcDirectory,
                    entityName,
                    entityPlural,
                    foreignEntityName,
                    foreignEntityPlural,
                    projectBaseName);
                return;
            }

            entityModifier.AddSingularRelationshipEntity(srcDirectory,
                    foreignEntityName,
                    foreignEntityPlural,
                    entityName,
                    entityPlural,
                    entityName,
                    projectBaseName);
        }
        
        public override void UpdateEntityManagementMethods(EntityModifier entityModifier, string srcDirectory, string entityName,
            string entityPlural, EntityProperty entityProperty, string projectBaseName)
        {
            if (IsChildRelationship)
            {
                entityModifier.AddEntitySingularManagementMethods(srcDirectory, entityProperty, entityName, entityPlural, projectBaseName);
                return;
            }
            entityModifier.AddEntityManyManagementMethods(srcDirectory, entityProperty, entityName, entityPlural, projectBaseName);
        }
    }

    private class ManyToOneType : DbRelationship
    {
        public ManyToOneType(bool isChildRelationship = false) : base("manyto1", 2, isChildRelationship) { }

        public override string GetEntityDbConfig(string entityName, string entityPlural, string propertyName,
            string foreignEntityPlural, string foreignEntityName)
        {
            if (IsChildRelationship)
                return @$"{Environment.NewLine}        builder.HasMany(x => x.{entityPlural})
            .WithOne(x => x.{propertyName});";
            
            return @$"{Environment.NewLine}        builder.HasOne(x => x.{propertyName})
            .WithMany(x => x.{entityPlural});";
        }

        public override string GetPrincipalPropString(string propertyType, string propertyName, string defaultValue,
            string foreignEntityName, string foreignEntityPlural, string entityName, string entityPlural)
        {
            if (IsChildRelationship)
            {
                var lowerPropName = entityPlural.LowercaseFirstLetter();
                return $@"    private readonly List<{entityName}> _{lowerPropName} = new();
    public IReadOnlyCollection<{entityName}> {entityPlural} => _{lowerPropName}.AsReadOnly();{Environment.NewLine}";
            }
            
            return $@"    public {foreignEntityName} {propertyName} {{ get; private set; }}{Environment.NewLine}";
        }

        public override void UpdateEntityProperties(EntityModifier entityModifier, string srcDirectory, string entityName,
            string entityPlural, string foreignEntityName, string foreignEntityPlural, string propertyName, string projectBaseName)
        {
            if (IsChildRelationship)
            {
                entityModifier.AddSingularRelationshipEntity(srcDirectory,
                    entityName,
                    entityPlural,
                    foreignEntityName,
                    foreignEntityPlural,
                    propertyName,
                    projectBaseName);
                return;
            }
            entityModifier.AddManyRelationshipEntity(srcDirectory,
                foreignEntityName,
                foreignEntityPlural,
                entityName,
                entityPlural,
                projectBaseName);
            
        }
        
        public override void UpdateEntityManagementMethods(EntityModifier entityModifier, string srcDirectory, string entityName,
            string entityPlural, EntityProperty entityProperty, string projectBaseName)
        {
            if (IsChildRelationship)
            {
                entityModifier.AddEntityManyManagementMethods(srcDirectory, entityProperty, entityName, entityPlural, projectBaseName);
                return;
            }
            entityModifier.AddEntitySingularManagementMethods(srcDirectory, entityProperty, entityName, entityPlural, projectBaseName);
        }
    }
    
    private class OneToOneType : DbRelationship
    {
        public OneToOneType(bool isChildRelationship = false) : base("1to1", 3, isChildRelationship) { }

        public override string GetEntityDbConfig(string entityName, string entityPlural, string propertyName,
            string foreignEntityPlural, string foreignEntityName)
        {
            if(IsChildRelationship)
                return @$"{Environment.NewLine}        builder.HasOne(x => x.{entityName})
            .WithOne(x => x.{propertyName})
            .HasForeignKey<{foreignEntityName}>(s => s.Id);";
            
            return @$"{Environment.NewLine}        builder.HasOne(x => x.{propertyName})
            .WithOne(x => x.{entityName})
            .HasForeignKey<{entityName}>(s => s.Id);";
        }

        public override void UpdateEntityProperties(EntityModifier entityModifier, string srcDirectory, string entityName,
            string entityPlural, string foreignEntityName, string foreignEntityPlural, string propertyName, string projectBaseName)
        {
            if (IsChildRelationship)
            {
            
                entityModifier.AddSingularRelationshipEntity(srcDirectory,
                    entityName,
                    entityPlural,
                    foreignEntityName,
                    foreignEntityPlural,
                    propertyName,
                    projectBaseName);
                return;
            }
            
            entityModifier.AddSingularRelationshipEntity(srcDirectory,
                foreignEntityName,
                foreignEntityPlural,
                entityName,
                entityPlural,
                entityName,
                projectBaseName);
        }
        
        public override void UpdateEntityManagementMethods(EntityModifier entityModifier, string srcDirectory, string entityName,
            string entityPlural, EntityProperty entityProperty, string projectBaseName)
        {
            // entityModifier.AddEntitySingularManagementMethods(srcDirectory, entityProperty, entityName, entityPlural, projectBaseName);
        }

        public override string GetPrincipalPropString(string propertyType, string propertyName, string defaultValue,
            string foreignEntityName, string foreignEntityPlural, string entityName, string entityPlural)
        {
            if(IsChildRelationship)
                return $@"    public {entityName} {entityName} {{ get; private set; }} = {entityName}.Create(new {EntityModel.Creation.GetClassName(entityName)}());{Environment.NewLine}";
            
            return $@"    public {foreignEntityName} {propertyName} {{ get; private set; }} = {foreignEntityName}.Create(new {EntityModel.Creation.GetClassName(foreignEntityName)}());{Environment.NewLine}";
        }
    }
    
    private class ManyToManyType : DbRelationship
    {
        public ManyToManyType(bool isChildRelationship = false) : base("manytomany", 4, isChildRelationship) { }

        public override string GetEntityDbConfig(string entityName, string entityPlural, string propertyName,
            string foreignEntityPlural, string foreignEntityName)
        {
            if(IsChildRelationship)
                return @$"{Environment.NewLine}        builder.HasMany(x => x.{entityPlural})
            .WithMany(x => x.{foreignEntityPlural});";
            
            return @$"{Environment.NewLine}        builder.HasMany(x => x.{propertyName})
            .WithMany(x => x.{entityPlural});";
        }
        public override string GetPrincipalPropString(string propertyType, string propertyName, string defaultValue, string foreignEntityName, string foreignEntityPlural, string entityName, string entityPlural)
        {
            if (IsChildRelationship)
            {
                var lowerPropNameForChild = entityPlural.LowercaseFirstLetter();
                return $@"    private readonly List<{entityName}> _{lowerPropNameForChild} = new();
    public IReadOnlyCollection<{entityName}> {entityPlural} => _{lowerPropNameForChild}.AsReadOnly();{Environment.NewLine}";
            }
            var lowerPropName = foreignEntityPlural.LowercaseFirstLetter();
            return $@"    private readonly List<{foreignEntityName}> _{lowerPropName} = new();
    public IReadOnlyCollection<{foreignEntityName}> {propertyName} => _{lowerPropName}.AsReadOnly();{Environment.NewLine}";
        }

        public override void UpdateEntityProperties(EntityModifier entityModifier, string srcDirectory, string entityName,
            string entityPlural, string foreignEntityName, string foreignEntityPlural, string propertyName, string projectBaseName)
        {
            if (IsChildRelationship)
            {
                entityModifier.AddManyRelationshipEntity(srcDirectory,
                    entityName,
                    entityPlural,
                    foreignEntityName,
                    foreignEntityPlural,
                    projectBaseName);
                return;
            }

            entityModifier.AddManyRelationshipEntity(srcDirectory,
                foreignEntityName,
                foreignEntityPlural,
                entityName,
                entityPlural,
                projectBaseName);
        }
        
        public override void UpdateEntityManagementMethods(EntityModifier entityModifier, string srcDirectory, string entityName,
            string entityPlural, EntityProperty entityProperty, string projectBaseName)
        {
            entityModifier.AddEntityManyManagementMethods(srcDirectory, entityProperty, entityName, entityPlural, projectBaseName);
        }
    }
    
    private class SelfType : DbRelationship
    {
        public SelfType(bool isChildRelationship = false) : base("self", 5, isChildRelationship) { }
        public override string GetEntityDbConfig(string entityName, string entityPlural, string propertyName, string foreignEntityPlural, string foreignEntityName)
            => @$"{Environment.NewLine}        builder.HasOne(x => x.{propertyName});";
        public override string GetPrincipalPropString(string propertyType, string propertyName, string defaultValue, string foreignEntityName, string foreignEntityPlural, string entityName, string entityPlural) 
            => $@"    public {foreignEntityName} {propertyName} {{ get; private set; }}{Environment.NewLine}";

        public override void UpdateEntityProperties(EntityModifier entityModifier, string srcDirectory,
            string entityName, string entityPlural, string foreignEntityName, string foreignEntityPlural, string propertyName, string projectBaseName)
        {
            // no op
        }
        
        public override void UpdateEntityManagementMethods(EntityModifier entityModifier, string srcDirectory, string entityName,
            string entityPlural, EntityProperty entityProperty, string projectBaseName)
        {
            // no op
        }
    }
}
