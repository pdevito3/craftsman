namespace NewCraftsman.Domain;

using Helpers;

public class BffEntity
{
    private string _name;
    /// <summary>
    /// The name of the entity
    /// </summary>
    public string Name
    {
        get => _name.UppercaseFirstLetter();
        set => _name = value;
    }

    private string _plural;
    /// <summary>
    /// The Plural name of the entity.
    /// </summary>
    public string Plural
    {
        get => _plural ?? $"{Name}s";
        set => _plural = value;
    }

    /// <summary>
    /// List of properties associated to the entity
    /// </summary>
    public List<BffEntityProperty> Properties { get; set; } = new List<BffEntityProperty>();
    
    private List<BffFeature> _features = new List<BffFeature>();
    public List<BffFeature> Features
    {
        get => _features;
        set
        {
            var entitylessFeatures = value ?? new List<BffFeature>();
            _features = entitylessFeatures
                .Select(f =>
                {
                    f.EntityName = Name;
                    f.EntityPlural = Plural;
                    return f;
                })
                .ToList();
        }
    }
}