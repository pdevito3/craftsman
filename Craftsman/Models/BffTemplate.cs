namespace Craftsman.Models;

using Enums;
using Exceptions;
using Helpers;

public class BffTemplate
{
    private string _projectName;
    public string ProjectName
    {
        get => _projectName;
        set => _projectName = value ?? throw new Exception("Project name is required for a BFF template.");
    }

    private string _profileName;
    public string ProfileName
    {
        get => _profileName;
        set => _profileName = value ?? ProjectName;
    }
    
    private int _port = Utilities.GetFreePort();
    /// <summary>
    /// The port of the .NET app. Will initially boot to this port and then forward to the SPA proxy
    /// </summary>
    public int? Port
    {
        get => _port;
        set => _port = value ?? _port;
    }
    
    private int _proxyPort = Utilities.GetFreePort();
    /// <summary>
    /// The port of your SPA
    /// </summary>
    
    //TODO this being nullable is really mixing a domain
    // concept with what should be a dto. the dto they pass should be a nullable
    // int, but the domain should separately take that in and be a nonnullable int when set
    public int? ProxyPort 
    {
        get => _proxyPort;
        set => _proxyPort = value ?? _proxyPort;
    }
    
    public string Authority { get; set; }

    public string ClientId { get; set; }

    public string ClientSecret { get; set; }

    private string _cookieName; 
    public string CookieName
    {
        get => _cookieName ?? $"__Host-{ProjectName}-bff";
        set => _cookieName = value;
    }

    public List<RemoteEndpoint> RemoteEndpoints { get; set; }

    public List<string> BoundaryScopes { get; set; }

    private string _headTitle; 
    public string HeadTitle
    {
        get => _headTitle;
        set => _headTitle = value ?? $"{ProjectName} App";
    }

    public List<BffEntity> Entities { get; set; }
}

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
        set => _type = Utilities.PropTypeCleanupTypeScript(value);
    }

    public bool Nullable => Type.Contains('?');

    /// <summary>
    /// The Type with the optional marker removed
    /// </summary>
    public string RawType => Type.Replace("?", "");
}