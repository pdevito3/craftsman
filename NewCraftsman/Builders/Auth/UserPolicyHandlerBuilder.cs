namespace NewCraftsman.Builders.Auth
{
    using System.IO.Abstractions;
    using Helpers;
    using Services;

    public class UserPolicyHandlerBuilder
    {
        private readonly ICraftsmanUtilities _utilities;

        public UserPolicyHandlerBuilder(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }
        
        public void CreatePolicyBuilder(string solutionDirectory, string srcDirectory, string projectBaseName, string dbContextName)
        {
            var classPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "UserPolicyHandler.cs", projectBaseName);
            var fileText = GetPolicyBuilderText(classPath.ClassNamespace, solutionDirectory, srcDirectory, dbContextName, projectBaseName);
            _utilities.CreateFile(classPath, fileText);
        }
        
        private static string GetPolicyBuilderText(string classNamespace, string solutionDirectory, string srcDirectory, string dbContextName, string projectBaseName)
        {
            var domainPolicyClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);
            var rolesClassPath = ClassPathHelper.SharedKernelDomainClassPath(solutionDirectory, "");
            var dbContextClassPath = ClassPathHelper.DbContextClassPath(srcDirectory, "UserPolicyHandler.cs", projectBaseName);
            
            return @$"namespace {classNamespace};

using System.Security.Claims;
using {dbContextClassPath.ClassNamespace};
using {rolesClassPath.ClassNamespace};
using {domainPolicyClassPath.ClassNamespace};
using HeimGuard;
using Microsoft.EntityFrameworkCore;

public class UserPolicyHandler : IUserPolicyHandler
{{
    private readonly {dbContextName} _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UserPolicyHandler({dbContextName} dbContext, ICurrentUserService currentUserService)
    {{
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }}
    
    public async Task<IEnumerable<string>> GetUserPermissions()
    {{
        var user = _currentUserService.User;
        if (user == null) throw new ArgumentNullException(nameof(user));

        var roles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(r => r.Value)
            .Distinct()
            .ToArray();
        
        // super admins can do everything
        if(roles.Contains(Roles.SuperAdmin))
            return Permissions.List();

        var permissions = await _dbContext.RolePermissions
            .Where(rp => roles.Contains(rp.Role))
            .Select(rp => rp.Permission)
            .Distinct()
            .ToArrayAsync();

        return await Task.FromResult(permissions);
    }}
}}";
        }
    }
}