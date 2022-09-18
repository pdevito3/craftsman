namespace Craftsman.Domain;

using Helpers;

public class BffEntityProperty
{
    private string _name;
    /// <summary>
    /// Name of the property
    /// </summary>
    public string Name
    {
        get => _name.UppercaseFirstLetter();
        set => _name = value;
    }

    private string _type = "string";
    /// <summary>
    /// Type of property (e.g. string, int, DateTime?, etc.)
    /// </summary>
    public string Type
    {
        get => _type;
        set => _type = CraftsmanUtilities.PropTypeCleanupTypeScript(value).TypescriptPropType();
    }

    public bool Nullable => Type.Contains('?');

    /// <summary>
    /// The Type with the optional marker removed
    /// </summary>
    public string RawType => Type.Replace("?", "");
}