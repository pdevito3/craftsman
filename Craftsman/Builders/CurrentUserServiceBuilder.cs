namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class CurrentUserServiceBuilder
    {
        public static void GetCurrentUserService(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "CurrentUserService.cs", projectBaseName);
            var fileText = GetCurrentUserServiceText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
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
}