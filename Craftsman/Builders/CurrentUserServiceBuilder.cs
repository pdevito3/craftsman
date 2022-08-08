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
        var fileText = GetCurrentUserServiceText(classPath.ClassNamespace, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetCurrentUserServiceText(string classNamespace, string srcDirectory, string projectBaseName)
    {
        var boundaryServiceName = FileNames.BoundaryServiceInterface(projectBaseName);

        return @$"namespace {classNamespace};

using System.Net;
using System.Security.Claims;

public interface ICurrentUserService : {boundaryServiceName}
{{
    string UserId {{ get; }}
    ClaimsPrincipal? User {{ get; }}
}}

public class CurrentUserService : ICurrentUserService
{{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {{
        _httpContextAccessor = httpContextAccessor;
    }}

    public string UserId
    {{
        get
        {{
            if (_httpContextAccessor.HttpContext != null)
                return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? Dns.GetHostEntry(_httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress)?.HostName;
            return ""UserNotFound"";
        }}
    }}

    public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;
}}";
    }
}
