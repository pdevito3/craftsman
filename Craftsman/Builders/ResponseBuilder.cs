namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System.IO.Abstractions;
    using System.Text;

    public class ResponseBuilder
    {
        public static void CreateResponse(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WrappersClassPath(solutionDirectory, $"Response.cs", projectBaseName);

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            if (fileSystem.File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (var fs = fileSystem.File.Create(classPath.FullClassPath))
            {
                var data = "";
                data = GetResponseText(classPath.ClassNamespace);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        public static string GetResponseText(string classNamespace)
        {
            return @$"namespace {classNamespace}
{{
    using System.Collections.Generic;

    public class Response<T>
    {{
        public Response()
        {{
        }}
        public Response(T data, string message = null)
        {{
            Succeeded = true;
            Message = message;
            Data = data;
        }}
        public Response(string message)
        {{
            Succeeded = false;
            Message = message;
        }}
        public bool Succeeded {{ get; set; }}
        public string Message {{ get; set; }}
        public List<string> Errors {{ get; set; }}
        public T Data {{ get; set; }}
    }}
}}";
        }
    }
}