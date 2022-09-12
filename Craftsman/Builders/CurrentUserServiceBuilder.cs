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

using System.Security.Claims;

public interface ICurrentUserService : {boundaryServiceName}
{{
    ClaimsPrincipal? User {{ get; }}
    string? UserId {{ get; }}
    string? Email {{ get; }}
    string? FirstName {{ get; }}
    string? LastName {{ get; }}
    string? Username {{ get; }}
    string? ClientId {{ get; }}
    bool IsMachine {{ get; }}
}}

public sealed class CurrentUserService : ICurrentUserService
{{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {{
        _httpContextAccessor = httpContextAccessor;
    }}

    public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;
    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
    public string? FirstName => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.GivenName);
    public string? LastName => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Surname);
    public string? Username => _httpContextAccessor.HttpContext
        ?.User
        ?.Claims
        ?.FirstOrDefault(x => x.Type is ""preferred_username"" or ""username"")
        ?.Value;
    public string? ClientId => _httpContextAccessor.HttpContext
        ?.User
        ?.Claims
        ?.FirstOrDefault(x => x.Type is ""client_id"" or ""clientId"")
        ?.Value;
    public bool IsMachine => ClientId != null;
}}";
    }
}
