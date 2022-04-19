namespace NewCraftsman.Builders
{
    using System.IO.Abstractions;

    public class ErrorHandlerFilterAttributeBuilder
    {
        public static void CreateErrorHandlerFilterAttribute(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiMiddlewareClassPath(srcDirectory, $"ErrorHandlerFilterAttribute.cs", projectBaseName);
            var fileText = GetErrorHandlerFilterAttributeText(srcDirectory, projectBaseName, classPath.ClassNamespace);
            _utilities.CreateFile(classPath, fileText);
        }

        public static string GetErrorHandlerFilterAttributeText(string srcDirectory, string projectBaseName, string classNamespace)
        {
            var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "");

            return @$"// source: https://github.com/jasontaylordev/CleanArchitecture/blob/main/src/WebUI/Filters/ApiExceptionFilterAttribute.cs

namespace {classNamespace};

using {exceptionsClassPath.ClassNamespace};
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ErrorHandlerFilterAttribute : ExceptionFilterAttribute
{{
    private readonly IDictionary<Type, Action<ExceptionContext>> _exceptionHandlers;

    public ErrorHandlerFilterAttribute()
    {{
        // Register known exception types and handlers.
        _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
            {{
                {{ typeof(FluentValidation.ValidationException), HandleFluentValidationException }},
                {{ typeof(ValidationException), HandleValidationException }},
                {{ typeof(NotFoundException), HandleNotFoundException }},
            }};
    }}

    public override void OnException(ExceptionContext context)
    {{
        HandleException(context);

        base.OnException(context);
    }}

    private void HandleException(ExceptionContext context)
    {{
        Type type = context.Exception.GetType();
        if (_exceptionHandlers.ContainsKey(type))
        {{
            _exceptionHandlers[type].Invoke(context);
            return;
        }}

        if (!context.ModelState.IsValid)
        {{
            HandleInvalidModelStateException(context);
            return;
        }}

        HandleUnknownException(context);
    }}

    private void HandleValidationException(ExceptionContext context)
    {{
        var exception = (ValidationException)context.Exception;
        HandleErrors(context, exception.Errors);
    }}
    
    private void HandleFluentValidationException(ExceptionContext context)
    {{
        var exception = (FluentValidation.ValidationException)context.Exception;
        var failures = exception.Errors
            .ToList();
        var proper = new ValidationException(failures);
        
        HandleErrors(context, proper.Errors);
    }}

    private void HandleErrors(ExceptionContext context, IDictionary<string, string[]> errors)
    {{
        var details = new ValidationProblemDetails(errors)
        {{
            Type = ""https://tools.ietf.org/html/rfc7231#section-6.5.1""
        }};

        context.Result = new BadRequestObjectResult(details);

        context.ExceptionHandled = true;
    }}

    private void HandleInvalidModelStateException(ExceptionContext context)
    {{
        var details = new ValidationProblemDetails(context.ModelState)
        {{
            Type = ""https://tools.ietf.org/html/rfc7231#section-6.5.1""
        }};

        context.Result = new BadRequestObjectResult(details);

        context.ExceptionHandled = true;
    }}

    private void HandleNotFoundException(ExceptionContext context)
    {{
        var exception = (NotFoundException)context.Exception;

        var details = new ProblemDetails()
        {{
            Type = ""https://tools.ietf.org/html/rfc7231#section-6.5.4"",
            Title = ""The specified resource was not found."",
            Detail = exception.Message
        }};

        context.Result = new NotFoundObjectResult(details);

        context.ExceptionHandled = true;
    }}

    private void HandleUnknownException(ExceptionContext context)
    {{
        var details = new ProblemDetails
        {{
            Status = StatusCodes.Status500InternalServerError,
            Title = ""An error occurred while processing your request."",
            Type = ""https://tools.ietf.org/html/rfc7231#section-6.6.1""
        }};

        context.Result = new ObjectResult(details)
        {{
            StatusCode = StatusCodes.Status500InternalServerError
        }};

        context.ExceptionHandled = true;
    }}
}}";
        }
    }
}