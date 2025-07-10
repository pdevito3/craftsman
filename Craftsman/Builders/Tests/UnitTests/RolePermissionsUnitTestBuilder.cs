namespace Craftsman.Builders.Tests.UnitTests;

using System.IO;
using Helpers;
using Services;
using MediatR;

public static class RolePermissionsUnitTestBuilder
{
    public sealed record CreateRolePermissionTestsCommand : IRequest;
    public sealed record UpdateRolePermissionTestsCommand : IRequest;

    public class CreateRolePermissionTestsHandler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<CreateRolePermissionTestsCommand>
    {
        public Task Handle(CreateRolePermissionTestsCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.UnitTestEntityTestsClassPath(scaffoldingDirectoryStore.TestDirectory, $"CreateRolePermissionTests.cs", "RolePermissions", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = CreateFileText(scaffoldingDirectoryStore.SolutionDirectory, scaffoldingDirectoryStore.SrcDirectory, classPath, scaffoldingDirectoryStore.ProjectBaseName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    public class UpdateRolePermissionTestsHandler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<UpdateRolePermissionTestsCommand>
    {
        public Task Handle(UpdateRolePermissionTestsCommand request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.UnitTestEntityTestsClassPath(scaffoldingDirectoryStore.TestDirectory, $"UpdateRolePermissionTests.cs", "RolePermissions", scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = UpdateFileText(scaffoldingDirectoryStore.SolutionDirectory, scaffoldingDirectoryStore.SrcDirectory, classPath, scaffoldingDirectoryStore.ProjectBaseName);
            utilities.CreateFile(classPath, fileText);
            return Task.CompletedTask;
        }
    }

    private static string CreateFileText(string solutionDirectory, string srcDirectory, ClassPath classPath, string projectBaseName)
    {
        var resourcesClassPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "", projectBaseName);
        var domainPolicyClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);
        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "RolePermissions", projectBaseName);
        var rolesClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "Roles", projectBaseName);
        var modelClassPath = ClassPathHelper.EntityModelClassPath(srcDirectory, "RolePermission", "RolePermissions", null, projectBaseName);
        var exceptionClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {domainPolicyClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {resourcesClassPath.ClassNamespace};
using {modelClassPath.ClassNamespace};
using {rolesClassPath.ClassNamespace};
using Bogus;
using ValidationException = {exceptionClassPath.ClassNamespace}.ValidationException;

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
        var newRolePermission = RolePermission.Create(new RolePermissionForCreation()
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
        var rolePermission = () => RolePermission.Create(new RolePermissionForCreation()
        {{
            Permission = _faker.PickRandom(Permissions.List()),
            Role = _faker.Lorem.Word()
        }});

        // Act + Assert
        rolePermission.Should().Throw<ValidationException>();
    }}
    
    [Fact]
    public void can_NOT_create_rolepermission_with_invalid_permission()
    {{
        // Arrange
        var rolePermission = () => RolePermission.Create(new RolePermissionForCreation()
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
        var resourcesClassPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "", projectBaseName);
        var domainPolicyClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);
        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "RolePermissions", projectBaseName);
        var rolesClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "Roles", projectBaseName);
        var modelClassPath = ClassPathHelper.EntityModelClassPath(srcDirectory, "RolePermission", "RolePermissions", null, projectBaseName);
        var exceptionClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {domainPolicyClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {resourcesClassPath.ClassNamespace};
using {modelClassPath.ClassNamespace};
using {rolesClassPath.ClassNamespace};
using Bogus;
using ValidationException = {exceptionClassPath.ClassNamespace}.ValidationException;

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
        var rolePermission = RolePermission.Create(new RolePermissionForCreation()
        {{
            Permission = _faker.PickRandom(Permissions.List()),
            Role = _faker.PickRandom(Role.ListNames())
        }});
        var permission = _faker.PickRandom(Permissions.List());
        var role = _faker.PickRandom(Role.ListNames());
        
        // Act
        rolePermission.Update(new RolePermissionForUpdate()
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
        var rolePermission = RolePermission.Create(new RolePermissionForCreation()
        {{
            Permission = _faker.PickRandom(Permissions.List()),
            Role = _faker.PickRandom(Role.ListNames())
        }});
        var updateRolePermission = () => rolePermission.Update(new RolePermissionForUpdate()
        {{
            Permission = _faker.PickRandom(Permissions.List()),
            Role = _faker.Lorem.Word()
        }});

        // Act + Assert
        updateRolePermission.Should().Throw<ValidationException>();
    }}
    
    [Fact]
    public void can_NOT_update_rolepermission_with_invalid_permission()
    {{
        // Arrange
        var rolePermission = RolePermission.Create(new RolePermissionForCreation()
        {{
            Permission = _faker.PickRandom(Permissions.List()),
            Role = _faker.PickRandom(Role.ListNames())
        }});
        var updateRolePermission = () => rolePermission.Update(new RolePermissionForUpdate()
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
