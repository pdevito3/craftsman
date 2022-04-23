namespace Craftsman.Domain;

public class RemoteEndpoint
{
    /// <summary>
    /// The path called in the spa (e.g. `/api/recipes`)
    /// </summary>
    public string LocalPath { get; set; }

    /// <summary>
    /// The path that the local path is forwarded to (e.g. `https://localhost:5375/api/recipes`)
    /// </summary>
    public string ApiAddress { get; set; }
    //TODO potentially add RequireAccessToken types
}