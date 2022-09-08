namespace Craftsman.Builders.Tests.Utilities;

using Helpers;
using Services;

public class HttpClientExtensionsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public HttpClientExtensionsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void Create(string solutionDirectory, string projectName)
    {
        var classPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(solutionDirectory, projectName, $"HttpClientExtensions.cs");
        var fileText = CreateHttpClientExtensionsText(classPath);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string CreateHttpClientExtensionsText(ClassPath classPath)
    {
        return @$"namespace {classPath.ClassNamespace};

using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

public static class HttpClientExtensions
{{
    public static HttpClient AddAuth(this HttpClient client, string nameIdentifier = null)
    {{
        nameIdentifier ??= Guid.NewGuid().ToString();
        var claims = new Dictionary<string, string>()
        {{
            {{ ClaimTypes.NameIdentifier , nameIdentifier }},
        }};
        
        client.SetFakeBearerToken(claims);

        return client;
    }}

    public static async Task<HttpResponseMessage> GetRequestAsync(this HttpClient client, string url)
    {{
        return await client.GetAsync(url).ConfigureAwait(false);
    }}

    public static async Task<HttpResponseMessage> DeleteRequestAsync(this HttpClient client, string url)
    {{
        return await client.DeleteAsync(url).ConfigureAwait(false);
    }}

    public static async Task<HttpResponseMessage> PostJsonRequestAsync(this HttpClient client, string url, object value)
    {{
        return await client.PostAsJsonAsync(url, value).ConfigureAwait(false);
    }}

    public static async Task<HttpResponseMessage> PutJsonRequestAsync(this HttpClient client, string url, object value)
    {{
        return await client.PutAsJsonAsync(url, value).ConfigureAwait(false);
    }}
}}";
    }
}
