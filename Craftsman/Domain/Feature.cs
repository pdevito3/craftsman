namespace Craftsman.Domain;

using System;
using Enums;
using Exceptions;
using Helpers;

public class Feature
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
                if (value.Equals("CreateRecord", StringComparison.InvariantCultureIgnoreCase))
                    FeatureType = FeatureType.AddRecord;
                else
                    throw new InvalidFeatureTypeException(value);
            }
            FeatureType = parsed;
        }
    }

    private string _featureName = null;
    public string Name
    {
        get => FeatureType.FeatureName(EntityName, _featureName);
        set => _featureName = value;
    }

    private string _command = null;
    public string Command
    {
        get => FeatureType.CommandName();
        set => _command = value;
    }

    private string _responseType = "bool";
    public string ResponseType
    {
        get => _responseType;
        set => _responseType = CraftsmanUtilities.PropTypeCleanupDotNet(value);
    }

    private string _permission;
    public string PermissionName
    {
        get => _permission ?? $"Can{Name}";
        set => _permission = value;
    }

    private string _batchPropName;
    /// <summary>
    /// The name of the property on the foreign entity you are doing a batch add on.
    /// </summary>
    public string BatchPropertyName
    {
        get => _batchPropName;
        set => _batchPropName = value.UppercaseFirstLetter();
    }

    private string _batchPropType;
    /// <summary>
    /// The data type of the the property you are doing the batch add on. This is for a FK property, so probably a Guid or int.
    /// </summary>
    public string BatchPropertyType
    {
        get => _batchPropType;
        set => _batchPropType = CraftsmanUtilities.PropTypeCleanupDotNet(value);
    }

    private string _parentEntity;
    /// <summary>
    /// The name of the parent entity that the FK you using is associated to. For example, if you had a FK of `EventId`, the parent entity might be `Event`. Leave null if you're not batching on a FK.
    /// </summary>
    public string ParentEntity
    {
        get => _parentEntity;
        set => _parentEntity = value.UppercaseFirstLetter();
    }

    /// <summary>
    /// The plural of the FK entity. Leave null if you're not batching on a FK.
    /// </summary>
    public string ParentEntityPlural { get; set; }

    /// <summary>
    /// Determines whether or not the feature is protected with an authorization policy attribute.
    /// </summary>
    public bool IsProtected { get; set; } = false;

    // feature role as command, producer, consumer in the future... dropped the ball on the OG implementation
}
