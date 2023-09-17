namespace Craftsman.Builders.Bff;

using Helpers;
using Services;

public class CorrelationMiddlewareBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public CorrelationMiddlewareBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateFile(string projectDirectory)
    {
        var classPath = ClassPathHelper.BffProjectRootClassPath(projectDirectory, $"CorrelationMiddleware.cs");
        var fileText = @$"namespace {classPath.ClassNamespace};

public class CorrelationMiddleware
{{
    private readonly RequestDelegate _next;

    public CorrelationMiddleware(RequestDelegate next)
    {{
        _next = next;
    }}

    public async Task InvokeAsync(HttpContext context)
    {{
        try
        {{
            await _next(context);
        }}
        catch (Exception ex)
        {{
            if (!IsCorrelationFailedException(ex)) throw;
            
            context.Response.Redirect(""/bff/login"");
        }}
    }}

    private bool IsCorrelationFailedException(Exception ex)
    {{
        return ex.InnerException?.Message.Contains(""Correlation failed"") == true
               || ex.Message.Contains(""Correlation failed"");
    }}
}}";
        _utilities.CreateFile(classPath, fileText);
    }
}
