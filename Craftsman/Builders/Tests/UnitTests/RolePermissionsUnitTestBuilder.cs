namespace Craftsman.Builders.Tests.UnitTests;

using System.IO;
using Helpers;
using Services;

public class RolePermissionsUnitTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public RolePermissionsUnitTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateRolePermissionTests(string solutionDirectory, string testDirectory, string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.UnitTestEntityTestsClassPath(testDirectory, $"CreateRolePermissionTests.cs", "RolePermissions", projectBaseName);
        var fileText = CreateFileText(solutionDirectory, srcDirectory, classPath, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    public void UpdateRolePermissionTests(string solutionDirectory, string testDirectory, string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.UnitTestEntityTestsClassPath(testDirectory, $"UpdateRolePermissionTests.cs", "RolePermissions", projectBaseName);
        var fileText = UpdateFileText(solutionDirectory, srcDirectory, classPath, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string CreateFileText(string solutionDirectory, string srcDirectory, ClassPath classPath, string projectBaseName)
    {
        var wrapperClassPath = ClassPathHelper.WrappersClassPath(srcDirectory, "", projectBaseName);
        var domainPolicyClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);
        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "RolePermissions", projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", "RolePermissions", projectBaseName);
        var rolesClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "Roles", projectBaseName);
        var errorsClassPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, "");

        return @$"namespace {classPath.ClassNamespace};

using {errorsClassPath.ClassNamespace};
using {domainPolicyClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {wrapperClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {rolesClassPath.ClassNamespace};
using Bogus;
using FluentAssertions;
using Xunit;
using ValidationException = {errorsClassPath.ClassNamespace}.ValidationException;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}
{{
    private readonly Faker _faker;

    public {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}()
    {{
        _faker = new Faker();
    }}
    
    [Fact]
    public void can_create_valid_rolepermission()
    {{
        // Arrange
        var permission = _faker.PickRandom(Permissions.List());
        var role = _faker.PickRandom(Role.ListNames());

        // Act
        var newRolePermission = RolePermission.Create(new RolePermissionForCreationDto()
        {{
            Permission = permission,
            Role = role
        }});
        
        // Assert
        newRolePermission.Permission.Should().Be(permission);
        newRolePermission.Role.Value.Should().Be(role);
    }}
    
    [Fact]
    public void can_NOT_create_rolepermission_with_invalid_role()
    {{
        // Arrange
        var rolePermission = () => RolePermission.Create(new RolePermissionForCreationDto()
        {{
            Permission = _faker.PickRandom(Permissions.List()),
            Role = _faker.Lorem.Word()
        }});

        // Act + Assert
        rolePermission.Should().Throw<InvalidSmartEnumPropertyName>();
    }}
    
    [Fact]
    public void can_NOT_create_rolepermission_with_invalid_permission()
    {{
        // Arrange
        var rolePermission = () => RolePermission.Create(new RolePermissionForCreationDto()
        {{
            Role = _faker.PickRandom(Role.ListNames()),
            Permission = _faker.Lorem.Word()
        }});

        // Act + Assert
        rolePermission.Should().Throw<ValidationException>();
    }}
}}";
    }

    private static string UpdateFileText(string solutionDirectory, string srcDirectory, ClassPath classPath, string projectBaseName)
    {
        var wrapperClassPath = ClassPathHelper.WrappersClassPath(srcDirectory, "", projectBaseName);
        var domainPolicyClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);
        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "RolePermissions", projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", "RolePermissions", projectBaseName);
        var rolesClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "Roles", projectBaseName);
        var errorsClassPath = ClassPathHelper.ExceptionsClassPath(solutionDirectory, "");

        return @$"namespace {classPath.ClassNamespace};

using {domainPolicyClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {wrapperClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {rolesClassPath.ClassNamespace};
using {errorsClassPath.ClassNamespace};
using Bogus;
using FluentAssertions;
using Xunit;
using ValidationException = {errorsClassPath.ClassNamespace}.ValidationException;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}
{{
    private readonly Faker _faker;

    public {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}()
    {{
        _faker = new Faker();
    }}
    
    [Fact]
    public void can_update_rolepermission()
    {{
        // Arrange
        var rolePermission = RolePermission.Create(new RolePermissionForCreationDto()
        {{
            Permission = _faker.PickRandom(Permissions.List()),
            Role = _faker.PickRandom(Role.ListNames())
        }});
        var permission = _faker.PickRandom(Permissions.List());
        var role = _faker.PickRandom(Role.ListNames());
        
        // Act
        rolePermission.Update(new RolePermissionForUpdateDto()
        {{
            Permission = permission,
            Role = role
        }});
        
        // Assert
        rolePermission.Permission.Should().Be(permission);
        rolePermission.Role.Value.Should().Be(role);
    }}
    
    [Fact]
    public void can_NOT_update_rolepermission_with_invalid_role()
    {{
        // Arrange
        var rolePermission = RolePermission.Create(new RolePermissionForCreationDto()
        {{
            Permission = _faker.PickRandom(Permissions.List()),
            Role = _faker.PickRandom(Role.ListNames())
        }});
        var updateRolePermission = () => rolePermission.Update(new RolePermissionForUpdateDto()
        {{
            Permission = _faker.PickRandom(Permissions.List()),
            Role = _faker.Lorem.Word()
        }});

        // Act + Assert
        updateRolePermission.Should().Throw<InvalidSmartEnumPropertyName>();
    }}
    
    [Fact]
    public void can_NOT_update_rolepermission_with_invalid_permission()
    {{
        // Arrange
        var rolePermission = RolePermission.Create(new RolePermissionForCreationDto()
        {{
            Permission = _faker.PickRandom(Permissions.List()),
            Role = _faker.PickRandom(Role.ListNames())
        }});
        var updateRolePermission = () => rolePermission.Update(new RolePermissionForUpdateDto()
        {{
            Permission = _faker.Lorem.Word(),
            Role = _faker.PickRandom(Role.ListNames())
        }});

        // Act + Assert
        updateRolePermission.Should().Throw<ValidationException>();
    }}
}}";
    }
}
