namespace Craftsman.Models;

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
    
    /// <summary>
    /// The port of your SPA
    /// </summary>
    public int ProxyPort { get; set; }
    
    /// <summary>
    /// The port of the .NET app. Will initially boot to this port and then forward to the SPA proxy
    /// </summary>
    public int Port { get; set; }
    
    
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