namespace Craftsman.Builders.Auth;

using Helpers;
using Services;

public class UserPolicyHandlerBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public UserPolicyHandlerBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreatePolicyBuilder(string solutionDirectory, string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "UserPolicyHandler.cs", projectBaseName);
        var fileText = GetPolicyBuilderText(classPath.ClassNamespace, solutionDirectory, srcDirectory, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetPolicyBuilderText(string classNamespace, string solutionDirectory, string srcDirectory, string projectBaseName)
    {
        var domainPolicyClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);
        var rolesClassPath = ClassPathHelper.SharedKernelDomainClassPath(solutionDirectory, "");
        var entityServices = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", "RolePermissions", projectBaseName);

        return @$"namespace {classNamespace};

using System.Security.Claims;
using {entityServices.ClassNamespace};
using {rolesClassPath.ClassNamespace};
using {domainPolicyClassPath.ClassNamespace};
using HeimGuard;
using Microsoft.EntityFrameworkCore;

public class UserPolicyHandler : IUserPolicyHandler
{{
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly ICurrentUserService _currentUserService;

    public UserPolicyHandler(IRolePermissionRepository rolePermissionRepository, ICurrentUserService currentUserService)
    {{
        _rolePermissionRepository = rolePermissionRepository;
        _currentUserService = currentUserService;
    }}
    
    public async Task<IEnumerable<string>> GetUserPermissions()
    {{
        var user = _currentUserService.User;
        if (user == null) throw new ArgumentNullException(nameof(user));

        var roles = user.Claims
            .Where(c => c.Type is ClaimTypes.Role or ""client_role"")
            .Select(r => r.Value)
            .Distinct()
            .ToArray();
        
        if(roles.Length == 0)
            return Array.Empty<string>();

        // super admins can do everything
        if(roles.Contains(Roles.SuperAdmin))
            return Permissions.List();

        var permissions = await _rolePermissionRepository.Query()
            .Where(rp => roles.Contains(rp.Role))
            .Select(rp => rp.Permission)
            .Distinct()
            .ToArrayAsync();

        return await Task.FromResult(permissions);
    }}
}}";
    }
}
