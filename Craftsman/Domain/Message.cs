namespace Craftsman.Domain;

using System.Collections.Generic;
using Helpers;

public class Message
{
    private string _name;

    /// <summary>
    /// The name of the message
    /// </summary>
    public string Name
    {
        get
        {
            // Fix recursion on null and normalize casing (Issue #145)
            var baseName = _name?.UppercaseFirstLetter();
            if (baseName != null && baseName.StartsWith("I") && baseName.Length > 1 && char.IsUpper(baseName[1]))
                baseName = baseName.Remove(0, 1);

            return baseName;
        }
        set => _name = value;
    }

    /// <summary>
    /// List of properties associated to the message
    /// </summary>
    public List<MessageProperty> Properties { get; set; } = new List<MessageProperty>();
}
