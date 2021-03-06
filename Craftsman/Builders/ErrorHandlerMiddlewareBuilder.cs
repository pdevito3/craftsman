namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class ErrorHandlerMiddlewareBuilder
    {
        public static void CreateErrorHandlerMiddleware(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            try
            {
                var classPath = ClassPathHelper.WebApiMiddlewareClassPath(solutionDirectory, $"ErrorHandlerMiddleware.cs", projectBaseName);

                if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                    fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

                if (fileSystem.File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (var fs = fileSystem.File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetErrorHandlerMiddlewareText(solutionDirectory, projectBaseName, classPath.ClassNamespace);
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

        public static string GetErrorHandlerMiddlewareText(string solutionDirectory, string projectBaseName, string classNamespace)
        {
            var wrappersClassPath = ClassPathHelper.SharedDtoClassPath(solutionDirectory, "", projectBaseName);
            var exceptionsClassPath = ClassPathHelper.SharedDtoClassPath(solutionDirectory, "", projectBaseName);

            return @$"namespace {classNamespace}
{{
    using {exceptionsClassPath.ClassNamespace};
    using {wrappersClassPath.ClassNamespace};
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class ErrorHandlerMiddleware
    {{
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {{
            _next = next;
        }}

        public async Task Invoke(HttpContext context)
        {{
            try
            {{
                await _next(context);
            }}
            catch (Exception error)
            {{
                var response = context.Response;
                response.ContentType = ""application/json"";
                var responseModel = new Response<string>() {{ Succeeded = false, Message = error?.Message }};

                switch (error)
            {{
                    case ApiException e:
                        // custom application error
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;
                    case ValidationException e:
                        // custom application error
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        responseModel.Errors = e.Errors;
                        break;
                    case KeyNotFoundException e:
                        // not found error
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    default:
                        // unhandled error
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }}
                var result = JsonSerializer.Serialize(responseModel);

                await response.WriteAsync(result);
            }}
        }}
    }}
}}";
        }
    }
}
