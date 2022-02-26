namespace Craftsman.Models;

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
        get => _cookieName;
        set => _cookieName = value ?? $"__Host-{ProjectName}-bff";
    }

    public List<RemoteEndpoint> RemoteEndpoints { get; set; }

    public List<string> BoundaryScopes { get; set; }

    private string _headTitle; 
    public string HeadTitle
    {
        get => _headTitle;
        set => _headTitle = value ?? $"{ProjectName} App";
    }
}