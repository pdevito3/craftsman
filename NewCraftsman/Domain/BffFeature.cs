namespace NewCraftsman.Domain;

using Enums;
using Exceptions;

public class BffFeature
{
    public string EntityName { get; set; }

    public string EntityPlural { get; set; }
        
    private FeatureType FeatureType { get; set; }
    public string Type
    {
        get => FeatureType.Name;
        set
        {
            if (!FeatureType.TryFromName(value, true, out var parsed))
            {
                if(value.Equals("CreateRecord", StringComparison.InvariantCultureIgnoreCase))
                    FeatureType = FeatureType.AddRecord;
                else
                    throw new InvalidFeatureTypeException(value);
            }
            FeatureType = parsed;
        }
    }

    public string BffApiName => FeatureType == FeatureType.GetList 
        ? FeatureType.BffApiName(EntityPlural) 
        : FeatureType.BffApiName(EntityName);
}