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

    public void CreateTests(string testDirectory, string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.UnitTestServiceTestsClassPath(testDirectory, "UserPolicyHandlerTests.cs", projectBaseName);
        var fileText = WriteTestFileText(srcDirectory, testDirectory, classPath, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string srcDirectory, string testDirectory, ClassPath classPath, string projectBaseName)
    {
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", "RolePermissions", projectBaseName);
        var policyDomainClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);
        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "RolePermissions", projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", "RolePermissions", projectBaseName);
        var rolesClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "Roles", projectBaseName);
        var userClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "Users", projectBaseName);
        var userServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", "Users", projectBaseName);
        var fakeUsersClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", "User", projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {userClassPath.ClassNamespace};
using {userServicesClassPath.ClassNamespace};
using {fakeUsersClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using {entityServicesClassPath.ClassNamespace};
using {policyDomainClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {rolesClassPath.ClassNamespace};
using Bogus;
using FluentAssertions;
using MediatR;
using MockQueryable.Moq;
using Moq;
using Xunit;
using System.Threading.Tasks;
using System.Security.Claims;

public class UserPolicyHandlerTests
{{
    private readonly Faker _faker;

    public UserPolicyHandlerTests()
    {{
        _faker = new Faker();
    }}

    [Fact]
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
        var userRepo = new Mock<IUserRepository>();
        var mediator = new Mock<IMediator>();

        var userPolicyHandler = new UserPolicyHandler(rolePermissionsRepo.Object, currentUserService.Object, userRepo.Object, mediator.Object);
        
        Func<Task> permissions = () => userPolicyHandler.GetUserPermissions();
        
        // Assert
        permissions.Should().ThrowAsync<ArgumentNullException>();
    }}
    
    [Fact]
    public async Task superadmin_user_gets_all_permissions()
    {{
        // Arrange
        var mediator = new Mock<IMediator>();
        var userRepo = new Mock<IUserRepository>();
        userRepo.UsersExist();
        userRepo.SetRole(Role.SuperAdmin().Value);
        
        var currentUserService = new Mock<ICurrentUserService>();
        currentUserService.SetCurrentUser();
        var rolePermissionsRepo = new Mock<IRolePermissionRepository>();

        // Act
        var userPolicyHandler = new UserPolicyHandler(rolePermissionsRepo.Object, currentUserService.Object, userRepo.Object, mediator.Object);
        var permissions = await userPolicyHandler.GetUserPermissions();
        
        // Assert
        permissions.Should().BeEquivalentTo(Permissions.List().ToArray());
    }}
    
    [Fact]
    public async Task superadmin_machine_gets_all_permissions()
    {{
        // Arrange
        var mediator = new Mock<IMediator>();
        var userRepo = new Mock<IUserRepository>();
        userRepo.UsersExist();
        var currentUserService = new Mock<ICurrentUserService>();
        currentUserService.SetMachine();
        var rolePermissionsRepo = new Mock<IRolePermissionRepository>();
        
        userRepo.SetRole(Role.SuperAdmin().Value);
    
        // Act
        var userPolicyHandler = new UserPolicyHandler(rolePermissionsRepo.Object, currentUserService.Object, userRepo.Object, mediator.Object);
        var permissions = await userPolicyHandler.GetUserPermissions();
        
        // Assert
        permissions.Should().BeEquivalentTo(Permissions.List().ToArray());
    }}
    
    [Fact]
    public async Task non_super_admin_gets_assigned_permissions_only()
    {{
        // Arrange
        var permissionToAssign = _faker.PickRandom(Permissions.List());
        var randomOtherPermission = _faker.PickRandom(Permissions.List().Where(p => p != permissionToAssign));
        var nonSuperAdminRole = _faker.PickRandom(Role.ListNames().Where(p => p != Role.SuperAdmin().Value));
        
        var currentUserService = new Mock<ICurrentUserService>();
        currentUserService.SetCurrentUser();
        
        var mediator = new Mock<IMediator>();
        var userRepo = new Mock<IUserRepository>();
        userRepo.UsersExist();
        userRepo.SetRole(nonSuperAdminRole);
    
        var rolePermission = RolePermission.Create(new RolePermissionForCreationDto()
        {{
            Role = nonSuperAdminRole,
            Permission = permissionToAssign
        }});
        var rolePermissions = new List<RolePermission>() {{rolePermission}};
        var mockData = rolePermissions.AsQueryable().BuildMock();
        var rolePermissionsRepo = new Mock<IRolePermissionRepository>();
        rolePermissionsRepo
            .Setup(c => c.Query())
            .Returns(mockData);
        
        // Act
    
        var userPolicyHandler = new UserPolicyHandler(rolePermissionsRepo.Object, currentUserService.Object, userRepo.Object, mediator.Object);
        var permissions = await userPolicyHandler.GetUserPermissions();
        
        // Assert
        permissions.Should().Contain(permissionToAssign);
        permissions.Should().NotContain(randomOtherPermission);
    }}
    
    [Fact]
    public async Task claims_role_duplicate_permissions_removed()
    {{
        // Arrange
        var permissionToAssign = _faker.PickRandom(Permissions.List());
        var nonSuperAdminRole = _faker.PickRandom(Role.ListNames().Where(p => p != Role.SuperAdmin().Value));
        
        var currentUserService = new Mock<ICurrentUserService>();
        currentUserService.SetCurrentUser();
        
        var mediator = new Mock<IMediator>();
        var userRepo = new Mock<IUserRepository>();
        userRepo.UsersExist();
        userRepo.SetRole(nonSuperAdminRole);
    
        var rolePermission = RolePermission.Create(new RolePermissionForCreationDto()
        {{
            Role = nonSuperAdminRole,
            Permission = permissionToAssign
        }});
        var rolePermissions = new List<RolePermission>() {{rolePermission, rolePermission}};
        var mockData = rolePermissions.AsQueryable().BuildMock();
        var rolePermissionsRepo = new Mock<IRolePermissionRepository>();
        rolePermissionsRepo
            .Setup(c => c.Query())
            .Returns(mockData);
        
        // Act
        var userPolicyHandler = new UserPolicyHandler(rolePermissionsRepo.Object, currentUserService.Object, userRepo.Object, mediator.Object);
        var permissions = await userPolicyHandler.GetUserPermissions();
        
        // Assert
        permissions.Count(p => p == permissionToAssign).Should().Be(1);
        permissions.Should().Contain(permissionToAssign);
    }}
}}

public static class UserExtensions
{{
    public static void SetRole(this Mock<IUserRepository> repo, string role)
    {{
        repo
            .Setup(x => x.GetRolesByUserIdentifier(It.IsAny<string>()))
            .Returns(new List<string> {{ role }});
    }}

    public static void UsersExist(this Mock<IUserRepository> repo)
    {{
        var user = FakeUser.Generate();
        var users = new List<User>() {{user}};
        var mockData = users.AsQueryable().BuildMock();
        
        repo
            .Setup(c => c.Query())
            .Returns(mockData);
    }}
}}
public static class CurrentUserServiceExtensions
{{
    public static void SetCurrentUser(this Mock<ICurrentUserService> repo, string nameIdentifier = null)
    {{
        var user = SetUserClaim(nameIdentifier);
        repo
            .Setup(c => c.User)
            .Returns(user);
        repo
            .Setup(c => c.UserId)
            .Returns(user?.FindFirstValue(ClaimTypes.NameIdentifier));
        repo
            .Setup(c => c.IsMachine)
            .Returns(false);
    }}
    
    public static void SetMachine(this Mock<ICurrentUserService> repo, string nameIdentifier = null, string clientId = null)
    {{
        var machine = SetMachineClaim(nameIdentifier, clientId);
        repo
            .Setup(c => c.User)
            .Returns(machine);
        repo
            .Setup(c => c.UserId)
            .Returns(machine?.FindFirstValue(ClaimTypes.NameIdentifier));
        repo
            .Setup(c => c.IsMachine)
            .Returns(true);
    }}
    
    private static ClaimsPrincipal SetUserClaim(string nameIdentifier = null)
    {{
        nameIdentifier ??= Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {{
            new Claim(ClaimTypes.NameIdentifier, nameIdentifier)
        }};

        var identity = new ClaimsIdentity(claims);
        return new ClaimsPrincipal(identity);
    }}
    
    private static ClaimsPrincipal SetMachineClaim(string nameIdentifier = null, string clientId = null)
    {{
        nameIdentifier ??= Guid.NewGuid().ToString();
        clientId ??= Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {{
            new Claim(ClaimTypes.NameIdentifier, nameIdentifier),
            new Claim(""clientId"", clientId)
        }};

        var identity = new ClaimsIdentity(claims);
        return new ClaimsPrincipal(identity);
    }}
}}";
    }
}
