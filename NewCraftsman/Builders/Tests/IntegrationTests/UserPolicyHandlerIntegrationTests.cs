namespace NewCraftsman.Builders.Tests.IntegrationTests
{
    using System.IO.Abstractions;
    using Helpers;
    using NewCraftsman.Services;

    public class UserPolicyHandlerIntegrationTests
    {
        private readonly ICraftsmanUtilities _utilities;

        public UserPolicyHandlerIntegrationTests(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }

        public void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, string projectBaseName)
        {
            var classPath = ClassPathHelper.ServicesTestClassPath(testDirectory, $"UserPolicyHandlerTests.cs", projectBaseName);
            var fileText =  WriteTestFileText(solutionDirectory, testDirectory, srcDirectory, classPath, projectBaseName);
            _utilities.CreateFile(classPath, fileText);
        }

        private static string WriteTestFileText(string solutionDirectory, string testDirectory, string srcDirectory, ClassPath classPath, string projectBaseName)
        {
            var testFixtureName = FileNames.GetIntegrationTestFixtureName();
            
            var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
            var policyDomainClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);
            var entityClassPath = ClassPathHelper.EntityClassPath(testDirectory, "", "RolePermissions", projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", "RolePermission", projectBaseName);
            var rolesClassPath = ClassPathHelper.SharedKernelDomainClassPath(solutionDirectory, "");

            return @$"namespace {classPath.ClassNamespace};

using {servicesClassPath.ClassNamespace};
using {policyDomainClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {rolesClassPath.ClassNamespace};
using Bogus;
using FluentAssertions;
using HeimGuard;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Security.Claims;
using static {testFixtureName};

public class UserPolicyHandlerTests : TestBase
{{
    private readonly Faker _faker;

    public UserPolicyHandlerTests()
    {{
        _faker = new Faker();
    }}
    
    [Test]
    public void GetUserPermissions_should_require_user()
    {{
        // Arrange
        var identity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = Mock.Of<HttpContext>(c => c.User == claimsPrincipal);
        var httpContextAccessor = GetService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = httpContext;
        
        // Act
        var userPolicyHandler = GetService<UserPolicyHandler>();
        Func<Task> permissions = () => userPolicyHandler.GetUserPermissions();
        
        // Assert
        permissions.Should().ThrowAsync<ArgumentNullException>();
    }}
    
    [Test]
    public async Task superadmin_gets_all_permissions()
    {{
        // Arrange
        SetUserRole(Roles.SuperAdmin);

        // Act
        var userPolicyHandler = GetService<IUserPolicyHandler>();
        var permissions = await userPolicyHandler.GetUserPermissions();
        
        // Assert
        permissions.Should().BeEquivalentTo(Permissions.List().ToArray());
    }}
    
    [Test]
    public async Task non_super_admin_gets_assigned_permissions_only()
    {{
        // Arrange
        var permissionToAssign = _faker.PickRandom(Permissions.List());
        var randomOtherPermission = _faker.PickRandom(Permissions.List().Where(p => p != permissionToAssign));
        var nonSuperAdminRole = _faker.PickRandom(Roles.List().Where(p => p != Roles.SuperAdmin));
        SetUserRole(nonSuperAdminRole);

        await InsertAsync(RolePermission.Create(new RolePermissionForCreationDto()
        {{
            Role = nonSuperAdminRole,
            Permission = permissionToAssign
        }}));
        
        // Act
        var userPolicyHandler = GetService<IUserPolicyHandler>();
        var permissions = await userPolicyHandler.GetUserPermissions();
        
        // Assert
        permissions.Should().Contain(permissionToAssign);
        permissions.Should().NotContain(randomOtherPermission);
    }}
    
    [Test]
    public async Task claims_role_duplicate_permissions_removed()
    {{
        // duplicates shouldn't be possible in the database OOTB with the RolePermission validation
        // (and unit tests), but leaving this for redundancy and safer modification possibilities
        
        // Arrange
        var permissionToAssign = _faker.PickRandom(Permissions.List());
        var nonSuperAdminRole = _faker.PickRandom(Roles.List().Where(p => p != Roles.SuperAdmin));
        SetUserRole(nonSuperAdminRole);

        await InsertAsync(RolePermission.Create(new RolePermissionForCreationDto()
        {{
            Role = nonSuperAdminRole,
            Permission = permissionToAssign
        }}));
        await InsertAsync(RolePermission.Create(new RolePermissionForCreationDto()
        {{
            Role = nonSuperAdminRole,
            Permission = permissionToAssign
        }}));
        
        // Act
        var userPolicyHandler = GetService<IUserPolicyHandler>();
        var permissions = await userPolicyHandler.GetUserPermissions();
        
        // Assert
        permissions.Should().HaveCount(1);
        permissions.Should().Contain(permissionToAssign);
    }}
}}";
        }
    }
}
