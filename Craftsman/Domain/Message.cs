namespace Craftsman.Domain;

using System.Collections.Generic;
using Exceptions;
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

    /// <summary>
    /// Combines messages declared in more than one place (domain root, bounded contexts) into one list per name.
    /// Identical declarations collapse to one. Different declarations with the same name are a template error.
    /// </summary>
    public static List<Message> MergeByName(IEnumerable<Message> messages)
    {
        return messages
            .Where(m => m != null && !string.IsNullOrWhiteSpace(m.Name))
            .GroupBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var first = group.First();
                if (group.Any(m => m.PropertySignature() != first.PropertySignature()))
                    throw new InvalidTemplateException(
                        $"The message `{first.Name}` is declared more than once with different properties. Use one definition for each message name.");
                return first;
            })
            .ToList();
    }

    private string PropertySignature()
        => string.Join(";", Properties.Select(p => $"{p.Name}:{p.Type}"));
}
