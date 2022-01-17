namespace Craftsman.Builders.Tests.UnitTests
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class RolePermissionsUnitTestBuilder
    {
        public static void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.UnitTestEntityTestsClassPath(testDirectory, $"RolePermissionTests.cs", "RolePermissions", projectBaseName);
            var fileText = WriteTestFileText(solutionDirectory, srcDirectory, classPath, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        private static string WriteTestFileText(string solutionDirectory, string srcDirectory, ClassPath classPath, string projectBaseName)
        {
            var wrapperClassPath = ClassPathHelper.WrappersClassPath(srcDirectory, "", projectBaseName);
            var domainPolicyClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);
            var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "RolePermissions", projectBaseName);
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", "RolePermission", projectBaseName);

            return @$"namespace {classPath.ClassNamespace};

using {domainPolicyClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {wrapperClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using Bogus;
using FluentAssertions;
using NUnit.Framework;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}
{{
    private readonly Faker _faker;

    public {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}()
    {{
        _faker = new Faker();
    }}
    
    [Test]
    public void can_create_valid_rolepermission()
    {{
        var permission = _faker.PickRandom(Permissions.List());
        var role = _faker.PickRandom(Roles.List());
        var newRolePermission = RolePermission.Create(new RolePermissionForCreationDto()
        {{
            Permission = permission,
            Role = role
        }});
        
        newRolePermission.Permission.Should().Be(permission);
        newRolePermission.Role.Should().Be(role);
    }}
    
    [Test]
    public void can_NOT_create_rolepermission_with_invalid_role()
    {{
        var rolePermission = () => RolePermission.Create(new RolePermissionForCreationDto()
        {{
            Permission = _faker.PickRandom(Permissions.List()),
            Role = _faker.Lorem.Word()
        }});
        rolePermission.Should().Throw<FluentValidation.ValidationException>();
    }}
    
    [Test]
    public void can_NOT_create_rolepermission_with_invalid_permission()
    {{
        var rolePermission = () => RolePermission.Create(new RolePermissionForCreationDto()
        {{
            Role = _faker.PickRandom(Roles.List()),
            Permission = _faker.Lorem.Word()
        }});
        rolePermission.Should().Throw<FluentValidation.ValidationException>();
    }}
}}";
        }
    }
}