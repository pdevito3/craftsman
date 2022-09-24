namespace Craftsman.Domain;

using Enums;
using Helpers;

public class NextJsEntityProperty
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

    private string _label;
    /// <summary>
    /// Label for form controls
    /// </summary>
    public string Label
    {
        get => _label.UppercaseFirstLetter() ?? Name.UppercaseFirstLetter();
        set => _label = value;
    }

    public TypescriptPropertyType TypeEnum { get; private set; } = TypescriptPropertyType.StringProperty;

    /// <summary>
    /// Type of property (e.g. string, number, Date?, etc.)
    /// </summary>
    public string Type
    {
        get => TypeEnum.TypescriptPropType();
        set => TypeEnum = CraftsmanUtilities.PropTypeCleanupTypeScript(value);
    }

    
    public FormControlType FormControlEnum { get; private set; } = FormControlType.Default;
    public string FormControl
    {
        get => FormControlEnum.Name;
        set
        {
            if (!FormControlType.TryFromName(value, true, out var parsed))
                FormControlEnum = FormControlType.Default;
            
            FormControlEnum = parsed;
        }
    }

    public bool Nullable => Type.Contains('?');

    /// <summary>
    /// The Type with the optional marker removed
    /// </summary>
    public string RawType => Type.Replace("?", "");
}