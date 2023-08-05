namespace Craftsman.Domain;

using Ardalis.SmartEnum;
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
    
    public bool IsNone => this == None;
    public bool IsOneToMany => this == OneToMany;
    public bool IsManyToOne => this == ManyToOne;
    public bool IsOneToOne => this == OneToOne;
    public bool IsManyToMany => this == ManyToMany;
    public bool IsSelf => this == Self;
    

    protected DbRelationship(string name, int value) : base(name, value)
    {
    }

    public abstract string GetPrincipalPropString(string propertyType, string propertyName, string defaultValue, string foreignEntityName, string foreignEntityPlural);
    public abstract string GetEntityDbConfig(string entityName, string entityPlural, string propertyName, string foreignEntityPlural);

    private class NoneType : DbRelationship
    {
        public NoneType() : base("none", 0) { }

        public override string GetEntityDbConfig(string entityName, string entityPlural, string propertyName, string foreignEntityPlural)
            => null;
        public override string GetPrincipalPropString(string propertyType, string propertyName, string defaultValue, string foreignEntityName, string foreignEntityPlural) 
            => $@"    public {propertyType} {propertyName} {{ get; private set; }}{defaultValue}{Environment.NewLine}{Environment.NewLine}";
    }

    private class OneToManyType : DbRelationship
    {
        public OneToManyType() : base("1tomany", 1) { }
        public override string GetEntityDbConfig(string entityName, string entityPlural, string propertyName, string foreignEntityPlural)
            => @$"{Environment.NewLine}        builder.HasMany(x => x.{foreignEntityPlural})
            .WithOne(x => x.{entityName});";
        public override string GetPrincipalPropString(string propertyType, string propertyName, string defaultValue, string foreignEntityName, string foreignEntityPlural)
        {
            var lowerPropName = foreignEntityPlural.LowercaseFirstLetter();
            return $@"    private readonly List<{foreignEntityName}> _{lowerPropName} = new();
    public IReadOnlyCollection<{foreignEntityName}> {foreignEntityPlural} => _{lowerPropName}.AsReadOnly();{Environment.NewLine}{Environment.NewLine}";
        }
    }

    private class ManyToOneType : DbRelationship
    {
        public ManyToOneType() : base("manyto1", 2) { }
        public override string GetEntityDbConfig(string entityName, string entityPlural, string propertyName, string foreignEntityPlural)
            => @$"{Environment.NewLine}        builder.HasOne(x => x.{propertyName})
            .WithMany(x => x.{entityPlural});";
        public override string GetPrincipalPropString(string propertyType, string propertyName, string defaultValue, string foreignEntityName, string foreignEntityPlural) 
            => $@"    public {foreignEntityName} {propertyName} {{ get; private set; }}{Environment.NewLine}{Environment.NewLine}";
    }
    
    private class OneToOneType : DbRelationship
    {
        public OneToOneType() : base("1to1", 3) { }
        public override string GetEntityDbConfig(string entityName, string entityPlural, string propertyName, string foreignEntityPlural)
            => @$"{Environment.NewLine}        builder.HasOne(x => x.{propertyName})
            .WithOne(x => x.{entityName})
            .HasForeignKey<{entityName}>(s => s.Id);";
        public override string GetPrincipalPropString(string propertyType, string propertyName, string defaultValue, string foreignEntityName, string foreignEntityPlural) 
            => $@"    public {foreignEntityName} {propertyName} {{ get; private set; }} = {foreignEntityName}.Create(new {EntityModel.Creation.GetClassName(foreignEntityName)}());{Environment.NewLine}{Environment.NewLine}";
    }
    
    private class ManyToManyType : DbRelationship
    {
        public ManyToManyType() : base("manytomany", 4) { }
        public override string GetEntityDbConfig(string entityName, string entityPlural, string propertyName, string foreignEntityPlural)
            => @$"{Environment.NewLine}        builder.HasMany(x => x.{foreignEntityPlural})
            .WithMany(x => x.{entityPlural});";
        public override string GetPrincipalPropString(string propertyType, string propertyName, string defaultValue, string foreignEntityName, string foreignEntityPlural)
        {
            var lowerPropName = foreignEntityName.LowercaseFirstLetter();
            return $@"    private readonly List<{foreignEntityName}> _{lowerPropName} = new();
    public IReadOnlyCollection<{foreignEntityName}> {foreignEntityPlural} => _{lowerPropName}.AsReadOnly();{Environment.NewLine}{Environment.NewLine}";
        }
    }
    
    private class SelfType : DbRelationship
    {
        public SelfType() : base("self", 5) { }
        public override string GetEntityDbConfig(string entityName, string entityPlural, string propertyName, string foreignEntityPlural)
            => @$"{Environment.NewLine}        builder.HasOne(x => x.{propertyName});";
        public override string GetPrincipalPropString(string propertyType, string propertyName, string defaultValue, string foreignEntityName, string foreignEntityPlural) 
            => $@"    public {foreignEntityName} {propertyName} {{ get; private set; }}{Environment.NewLine}{Environment.NewLine}";
    }
}
