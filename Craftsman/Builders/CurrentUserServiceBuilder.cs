namespace Craftsman.Builders;

using Helpers;
using Services;

public class CurrentUserServiceBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public CurrentUserServiceBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }
    public void GetCurrentUserService(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "CurrentUserService.cs", projectBaseName);
        var fileText = GetCurrentUserServiceText(classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetCurrentUserServiceText(string classNamespace)
    {
        return @$"namespace {classNamespace};

using System.Security.Claims;

public interface ICurrentUserService
{{
    string? UserId {{ get; }}
    ClaimsPrincipal? User {{ get; }}
}}

public class CurrentUserService : ICurrentUserService
{{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {{
        _httpContextAccessor = httpContextAccessor;
    }}

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;
}}";
    }
}
