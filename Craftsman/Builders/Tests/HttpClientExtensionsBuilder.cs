namespace Craftsman.Builders.Tests.IntegrationTests
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class HttpClientExtensionsBuilder
    {
        public static void Create(string solutionDirectory, string solutionName)
        {
            try
            {
                var classPath = ClassPathHelper.HttpClientExtensionsClassPath(solutionDirectory, solutionName, $"HttpClientExtensions.cs");

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    File.Delete(classPath.FullClassPath); // saves me from having to make a remover!

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = CreateHttpClientExtensionsText(classPath);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        private static string CreateHttpClientExtensionsText(ClassPath classPath)
        {
            return @$"
namespace {classPath.ClassNamespace}
{{
    using System;
    using System.Dynamic;
    using System.Net;
    using System.Net.Http;

    public static class HttpClientExtensions
    {{
        public static HttpClient AddAuth(this HttpClient client, string[] scopes)
        {{
            dynamic data = new ExpandoObject();
            data.sub = Guid.NewGuid();
            data.scope = scopes;
            client.SetFakeBearerToken((object)data);

            return client;
        }}
    }}
}}";
        }
    }
}
