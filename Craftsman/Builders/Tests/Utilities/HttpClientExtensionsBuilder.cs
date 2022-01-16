﻿namespace Craftsman.Builders.Tests.Utilities
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.Text;

    public class HttpClientExtensionsBuilder
    {
        public static void Create(string solutionDirectory, string projectName)
        {
            var classPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(solutionDirectory, projectName, $"HttpClientExtensions.cs");

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                File.Delete(classPath.FullClassPath); // saves me from having to make a remover!

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = CreateHttpClientExtensionsText(classPath);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        private static string CreateHttpClientExtensionsText(ClassPath classPath)
        {
            return @$"namespace {classPath.ClassNamespace};

using Microsoft.AspNetCore.JsonPatch;
using System.Text.Json;
using System.Dynamic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

public static class HttpClientExtensions
{{
    public static HttpClient AddAuth(this HttpClient client, params string[] roles)
    {{
        dynamic data = new ExpandoObject();
        data.sub = Guid.NewGuid();
        data.role = roles;
        client.SetFakeBearerToken((object)data);

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

    public static async Task<HttpResponseMessage> PatchJsonRequestAsync<TModel>(this HttpClient client, string url, JsonPatchDocument<TModel> patchDoc)
        where TModel : class
    {{
        var serializedRecipeToUpdate = JsonSerializer.Serialize(patchDoc);

        var patchRequest = new HttpRequestMessage(new HttpMethod(""PATCH""), url)
        {{
            Content = new StringContent(serializedRecipeToUpdate, Encoding.Unicode, ""application/json"")
        }};

        return await client.SendAsync(patchRequest).ConfigureAwait(false);
    }}
}}";
        }
    }
}
