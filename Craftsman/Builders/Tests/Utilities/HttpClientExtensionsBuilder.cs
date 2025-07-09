namespace Craftsman.Builders.Tests.Utilities;

using Helpers;
using Services;
using MediatR;

public static class HttpClientExtensionsBuilder
{
    public sealed record Command(string ProjectName) : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(scaffoldingDirectoryStore.SolutionDirectory, request.ProjectName, $"HttpClientExtensions.cs");
            var fileText = CreateHttpClientExtensionsText(classPath);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string CreateHttpClientExtensionsText(ClassPath classPath)
    {
        return @$"namespace {classPath.ClassNamespace};

using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Extensions.Services;

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
        var options = new JsonSerializerOptions();
        return await client.PostAsJsonAsync(url, value, options).ConfigureAwait(false);
    }}

    public static async Task<HttpResponseMessage> PutJsonRequestAsync(this HttpClient client, string url, object value)
    {{
        var options = new JsonSerializerOptions();
        return await client.PutAsJsonAsync(url, value, options).ConfigureAwait(false);
    }}
}}";
    }
}
