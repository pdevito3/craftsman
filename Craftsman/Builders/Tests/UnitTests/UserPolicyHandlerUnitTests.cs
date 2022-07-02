namespace Craftsman.Builders.Tests.UnitTests;

using Helpers;
using Services;

public class UserPolicyHandlerUnitTests
{
    private readonly ICraftsmanUtilities _utilities;

    public UserPolicyHandlerUnitTests(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.UnitTestServiceTestsClassPath(testDirectory, "UserPolicyHandlerTests.cs", projectBaseName);
        var fileText = WriteTestFileText(solutionDirectory, testDirectory, srcDirectory, classPath, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string solutionDirectory, string testDirectory, string srcDirectory, ClassPath classPath, string projectBaseName)
    {
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", "RolePermissions", projectBaseName);
        var policyDomainClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);
        var entityClassPath = ClassPathHelper.EntityClassPath(testDirectory, "", "RolePermissions", projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", "RolePermissions", projectBaseName);
        var rolesClassPath = ClassPathHelper.SharedKernelDomainClassPath(solutionDirectory, "");

        return @$"namespace {classPath.ClassNamespace};

using {servicesClassPath.ClassNamespace};
using {entityServicesClassPath.ClassNamespace};
using {policyDomainClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {rolesClassPath.ClassNamespace};
using Bogus;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Security.Claims;

[Parallelizable]
public class UserPolicyHandlerTests
{{
    private readonly Faker _faker;

    public UserPolicyHandlerTests()
    {{
        _faker = new Faker();
    }}

    public static ClaimsPrincipal SetUserRole(string role, string sub = null)
    {{
        sub ??= Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {{
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.Name, sub)
        }};

        var identity = new ClaimsIdentity(claims);
        return new ClaimsPrincipal(identity);
    }}
    
    public static ClaimsPrincipal SetMachineRole(string role, string clientId = null)
    {{
        clientId ??= Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {{
            new Claim(""client_role"", role),
            new Claim(""client_id"", clientId)
        }};

        var identity = new ClaimsIdentity(claims);
        return new ClaimsPrincipal(identity);
    }}
    
    [Test]
    public void GetUserPermissions_should_require_user()
    {{
        // Arrange
        var identity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        // Act
        var currentUserService = new Mock<ICurrentUserService>();
        currentUserService
            .Setup(c => c.User)
            .Returns(claimsPrincipal);
        var rolePermissionsRepo = new Mock<IRolePermissionRepository>();

        var userPolicyHandler = new UserPolicyHandler(rolePermissionsRepo.Object, currentUserService.Object);
        
        Func<Task> permissions = () => userPolicyHandler.GetUserPermissions();
        
        // Assert
        permissions.Should().ThrowAsync<ArgumentNullException>();
    }}
    
    [Test]
    public async Task superadmin_user_gets_all_permissions()
    {{
        // Arrange
        var user = SetUserRole(Roles.SuperAdmin);

        // Act
        var currentUserService = new Mock<ICurrentUserService>();
        currentUserService
            .Setup(c => c.User)
            .Returns(user);
        var rolePermissionsRepo = new Mock<IRolePermissionRepository>();

        var userPolicyHandler = new UserPolicyHandler(rolePermissionsRepo.Object, currentUserService.Object);
        var permissions = await userPolicyHandler.GetUserPermissions();
        
        // Assert
        permissions.Should().BeEquivalentTo(Permissions.List().ToArray());
    }}
    
    [Test]
    public async Task superadmin_machine_gets_all_permissions()
    {{
        // Arrange
        var user = SetMachineRole(Roles.SuperAdmin);

        // Act
        var currentUserService = new Mock<ICurrentUserService>();
        currentUserService
            .Setup(c => c.User)
            .Returns(user);
        var rolePermissionsRepo = new Mock<IRolePermissionRepository>();

        var userPolicyHandler = new UserPolicyHandler(rolePermissionsRepo.Object, currentUserService.Object);
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
        var user = SetUserRole(nonSuperAdminRole);

        var rolePermission = RolePermission.Create(new RolePermissionForCreationDto()
        {{
            Role = nonSuperAdminRole,
            Permission = permissionToAssign
        }});
        var rolePermissions = new List<RolePermission>() {{rolePermission}};
        var mockData = rolePermissions.AsQueryable().BuildMock();
        
        // Act
        var currentUserService = new Mock<ICurrentUserService>();
        currentUserService
            .Setup(c => c.User)
            .Returns(user);
        var rolePermissionsRepo = new Mock<IRolePermissionRepository>();
        rolePermissionsRepo
            .Setup(c => c.Query())
            .Returns(mockData);

        var userPolicyHandler = new UserPolicyHandler(rolePermissionsRepo.Object, currentUserService.Object);
        var permissions = await userPolicyHandler.GetUserPermissions();
        
        // Assert
        permissions.Should().Contain(permissionToAssign);
        permissions.Should().NotContain(randomOtherPermission);
    }}
    
    [Test]
    public async Task claims_role_duplicate_permissions_removed()
    {{
        // Arrange
        var permissionToAssign = _faker.PickRandom(Permissions.List());
        var nonSuperAdminRole = _faker.PickRandom(Roles.List().Where(p => p != Roles.SuperAdmin));
        var user = SetUserRole(nonSuperAdminRole);
    
        var rolePermission = RolePermission.Create(new RolePermissionForCreationDto()
        {{
            Role = nonSuperAdminRole,
            Permission = permissionToAssign
        }});
        var rolePermissions = new List<RolePermission>() {{rolePermission, rolePermission}};
        var mockData = rolePermissions.AsQueryable().BuildMock();
        
        // Act
        var currentUserService = new Mock<ICurrentUserService>();
        currentUserService
            .Setup(c => c.User)
            .Returns(user);
        var rolePermissionsRepo = new Mock<IRolePermissionRepository>();
        rolePermissionsRepo
            .Setup(c => c.Query())
            .Returns(mockData);

        var userPolicyHandler = new UserPolicyHandler(rolePermissionsRepo.Object, currentUserService.Object);
        var permissions = await userPolicyHandler.GetUserPermissions();
        
        // Assert
        permissions.Count(p => p == permissionToAssign).Should().Be(1);
        permissions.Should().Contain(permissionToAssign);
    }}
}}";
    }
}
