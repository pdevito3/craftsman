namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System;
    using System.IO.Abstractions;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class ResponseBuilder
    {
        public static void CreateResponse(string solutionDirectory, IFileSystem fileSystem)
        {
            try
            {
                var classPath = ClassPathHelper.WrappersClassPath(solutionDirectory, $"Response.cs");

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

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{fileSystem.Path.DirectorySeparatorChar}", ""));
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