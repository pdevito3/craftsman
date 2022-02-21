namespace Craftsman.Models;

public class AuthSettings
{

    public string Authority { get; set; }

    public string Audience { get; set; }

    public string AuthorizationUrl { get; set; }

    public string TokenUrl { get; set; }

    public string ClientId { get; set; }

    public string ClientSecret { get; set; }
}