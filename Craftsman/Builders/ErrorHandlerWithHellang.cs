namespace Craftsman.Builders;

using Helpers;
using Services;

public class ErrorHandlerWithHellang
{
    private readonly ICraftsmanUtilities _utilities;

    public ErrorHandlerWithHellang(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateErrorHandler(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiMiddlewareClassPath(srcDirectory, $"ProblemDetailsConfigurationExtension.cs", projectBaseName);
        var fileText = GetErrorHandlerText(srcDirectory, classPath.ClassNamespace, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetErrorHandlerText(string srcDirectory, string classNamespace, string projectBaseName)
    {
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);

        return @$"// source: https://github.com/jasontaylordev/CleanArchitecture/blob/main/src/WebUI/Filters/ApiExceptionFilterAttribute.cs

namespace {classNamespace};

using Hellang.Middleware.ProblemDetails;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using {exceptionsClassPath.ClassNamespace};
using ProblemDetailsOptions = Hellang.Middleware.ProblemDetails.ProblemDetailsOptions;

public static class ProblemDetailsConfigurationExtension
{{ 
    public static void ConfigureProblemDetails(ProblemDetailsOptions options)
    {{
        options.MapFluentValidationException();
        options.MapValidationException();
        options.MapToStatusCode<ForbiddenAccessException>(StatusCodes.Status401Unauthorized);
        options.MapToStatusCode<NoRolesAssignedException>(StatusCodes.Status403Forbidden);
        options.MapToStatusCode<NotFoundException>(StatusCodes.Status404NotFound);

        // You can configure the middleware to re-throw certain types of exceptions, all exceptions or based on a predicate.
        // This is useful if you have upstream middleware that needs to do additional handling of exceptions.
        // options.Rethrow<NotSupportedException>();

        options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);
        options.MapToStatusCode<HttpRequestException>(StatusCodes.Status503ServiceUnavailable);

        // You can configure the middleware to re-throw certain types of exceptions, all exceptions or based on a predicate.
        // This is useful if you have upstream middleware that  needs to do additional handling of exceptions.
        // options.Rethrow<NotSupportedException>();

        // You can configure the middleware to ignore any exceptions of the specified type.
        // This is useful if you have upstream middleware that  needs to do additional handling of exceptions.
        // Note that unlike Rethrow, additional information will not be added to the exception.
        // options.Ignore<DivideByZeroException>();

        // Because exceptions are handled polymorphically, this will act as a ""catch all"" mapping, which is why it's added last.
        // If an exception other than NotImplementedException and HttpRequestException is thrown, this will handle it.
        options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
    }}
    
    private static void MapFluentValidationException(this ProblemDetailsOptions options) =>
        options.Map<FluentValidation.ValidationException>((ctx, ex) =>
        {{
            var factory = ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();

            var errors = ex.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(x => x.ErrorMessage).ToArray());

            return factory.CreateValidationProblemDetails(ctx, errors);
        }});
    
    private static void MapValidationException(this ProblemDetailsOptions options) =>
        options.Map<ValidationException>((ctx, ex) =>
        {{
            var factory = ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();

            var errors = ex.Errors
                .GroupBy(x => x.Key)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(x => x.Value.ToString()).ToArray());

            return factory.CreateValidationProblemDetails(ctx, errors);
        }});
}}
";
    }
}
