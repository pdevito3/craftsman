namespace Craftsman.Builders.Tests.IntegrationTests;

using Craftsman.Services;
using Helpers;

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
        var fileText = WriteTestFileText(solutionDirectory, testDirectory, srcDirectory, classPath, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string solutionDirectory, string testDirectory, string srcDirectory, ClassPath classPath, string projectBaseName)
    {
        var testFixtureName = FileNames.GetIntegrationTestFixtureName();

        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        var policyDomainClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);
        var entityClassPath = ClassPathHelper.EntityClassPath(testDirectory, "", "RolePermissions", projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", "RolePermissions", projectBaseName);
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
    public async Task user_can_get_assigned_permissions()
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
}}";
    }
}
