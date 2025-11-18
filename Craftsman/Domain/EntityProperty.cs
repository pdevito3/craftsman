namespace Craftsman.Domain;

using Enums;
using Helpers;
using Humanizer;

public class EntityProperty
{
    private bool _isRequired = false;
    private bool _canManipulate = true;
    private string _type = "string";
    private string _name;

    public string Name
    {
        get => _name.UppercaseFirstLetter();
        set => _name = value;
    }
    public string Type
    {
        get
        {
            if (ValueObjectType.IsMonetaryAmount || ValueObjectType.IsPercent)
            {
                _type = CraftsmanUtilities.PropTypeCleanupDotNet("decimal");
            }

            return CraftsmanUtilities.PropTypeCleanupDotNet(_type);
        }
        set => _type = value;
        
    }
    public bool CanManipulate
    {
        get => _canManipulate;
        set => _canManipulate = IsForeignKey ? false : value;
    }
    public bool IsRequired
    {
        get => _isRequired;
        set => _isRequired = value;
    }

    /// <summary>
    /// Designates the property as a foreign key for the entity
    /// </summary>
    public bool IsForeignKey
    {
        get => !string.IsNullOrEmpty(ForeignEntityName);
    }
    
    public bool IsStringArray => Type.ToLowerInvariant()
        .Trim()
        .Replace("?", "") == "string[]";
    
    public string AsValueObject { get; set; }
    
    private string _valueObjectName; 
    public string ValueObjectName
    {
        get
        {
            return AsValueObject switch
            {
                "Email" => "Email",
                "Percent" => "Percent",
                "MonetaryAmount" => "MonetaryAmount",
                _ => _valueObjectName ?? Name
            };
        }
        set => _valueObjectName = value;
    }
    private string _valueObjectTypePlural; 
    public string ValueObjectPlural
    {
        get
        {
            if (AsValueObject != "Percent")
            {
                return _valueObjectTypePlural ?? ValueObjectName.Pluralize();
            }

            _valueObjectTypePlural = "Percentages";
            return _valueObjectTypePlural;
        }
        set => _valueObjectTypePlural = value;
    }

    public ValueObjectPropertyType ValueObjectType => CoreGetValueObjectPropertyType();
    private ValueObjectPropertyType CoreGetValueObjectPropertyType() 
    {
        var parsed = ValueObjectPropertyType.None(ValueObjectName);
        if (IsSmartEnum())
            return ValueObjectPropertyType.Smart(ValueObjectName);
        
        // TODO temp
        if(AsValueObject == "Simple")
            return ValueObjectPropertyType.Simple(ValueObjectName);
        
        if(AsValueObject == "Email")
            return ValueObjectPropertyType.Email(ValueObjectName);
        
        if(AsValueObject == "Percent")
            return ValueObjectPropertyType.Percent(ValueObjectName);
        
        if(AsValueObject == "MonetaryAmount")
            return ValueObjectPropertyType.MonetaryAmount(ValueObjectName);
        
        // if (!ValueObjectPropertyType.TryFromName(AsValueObject, true, out var parsed))
        //     parsed = ValueObjectPropertyType.None(ValueObjectName);
        
        return parsed;
    }

    public bool IsValueObject => !ValueObjectType.IsNone;

    public bool IsPrimitiveType
    {
        get
        {
            if (IsValueObject) return false;
            
            var rawType = Type.ToLowerInvariant().Trim().Replace("?", "");
            return rawType is "string"
                or "byte"
                or "sbyte"
                or "short"
                or "ushort"
                or "int"
                or "nint"
                or "uint"
                or "nuint"
                or "long"
                or "ulong"
                or "double"
                or "float"
                or "decimal"
                or "char"
                or "bool"
                or "dateonly"
                or "timeonly"
                or "datetime"
                or "datetimeoffset"
                or "timespan"
                or "object"
                or "guid";
        }
    }

    public bool IsChildRelationship { get; set; } = false;
    private string _relationship;
    public string Relationship
    {
        get => _relationship;
        set => _relationship = value.ToLowerInvariant().Replace("one", "1");
    }
    public DbRelationship GetDbRelationship => GetDbRelationshipFromName();
    
    private DbRelationship GetDbRelationshipFromName()
    {
        if (string.IsNullOrEmpty(Relationship))
            return DbRelationship.NoRelationship(IsChildRelationship);

        
        if (!DbRelationship.TryFromName(Relationship, true, out var parsed))
            parsed = DbRelationship.None;
        
        parsed.SetChildRelationship(IsChildRelationship);
        return parsed;
    }

    /// <summary>
    /// Captures the name of the entity this property is linked to as a foreign key.
    /// </summary>
    public string ForeignEntityName { get; set; }

    private string _plural;
    /// <summary>
    /// The plural value for the foreign entity, if applicable. Defaults to ForeignEntityName with an appended 's'.
    /// </summary>
    public string ForeignEntityPlural
    {
        get => _plural ?? $"{ForeignEntityName}s";
        set => _plural = value;
    }

    /// <summary>
    /// Default value for this property
    /// </summary>
    public string DefaultValue { get; set; }

    /// <summary>
    /// Database field name to use when it doesn't match the property name
    /// </summary>
    public string ColumnName { get; set; }

    /// <summary>
    /// Database type to use when it doesn't match the property type
    /// </summary>
    public string ColumnType { get; set; }

    public bool IsSmartEnum() => SmartNames.Count > 0;

    private string _smartEnumPropName;
    public string SmartEnumPropName
    {
        get => _smartEnumPropName ?? $"{Name}Enum"; 
        set => _smartEnumPropName = value;
    }

    public List<string> SmartNames { get; set; } = new List<string>();
    
    public bool IsLogMasked { get; set; }

    public List<SmartOption> GetSmartOptions() => SmartNames.Select((name, index) => new SmartOption { Name = name, Value = index}).ToList();

    public static EntityProperty GetPrimaryKey()
    {
        return new()
        {
            ColumnName = "id",
            IsRequired = true,
            CanManipulate = false,
            Name = "Id",
            Type = "Guid"
        };
    }

    /// <summary>
    /// The summary used in the documentation - visible in swagger
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// An example value to fill swagger example
    /// </summary>
    public string Example { get; set; }
}

public class SmartOption
{
    public string Name { get; set; }
    public int Value { get; set; }
}
