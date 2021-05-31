namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System.IO.Abstractions;
    using System.Text;

    public class ErrorHandlerMiddlewareBuilder
    {
        public static void CreateErrorHandlerMiddleware(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
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
        }

        public static string GetErrorHandlerMiddlewareText(string solutionDirectory, string projectBaseName, string classNamespace)
        {
            var wrappersClassPath = ClassPathHelper.WrappersClassPath(solutionDirectory, "", projectBaseName);
            var exceptionsClassPath = ClassPathHelper.CoreExceptionClassPath(solutionDirectory, "", projectBaseName);

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
                    case ConflictException:
                        response.StatusCode = (int)HttpStatusCode.Conflict;
                        break;

                    case ApiException:
                        // custom application error
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;

                    case ValidationException e:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        responseModel.Errors = e.Errors;
                        break;

                    case KeyNotFoundException:
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