namespace Craftsman.Domain;

using Helpers;

public class NextJsEntity
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

    private string _icon;
    /// <summary>
    /// Icon for navigation
    /// </summary>
    public string Icon
    {
        get => _icon.UppercaseFirstLetter() ?? "IconFolder";
        set => _icon = value;
    }

    /// <summary>
    /// List of properties associated to the entity
    /// </summary>
    public List<NextJsEntityProperty> Properties { get; set; } = new List<NextJsEntityProperty>();
    
}