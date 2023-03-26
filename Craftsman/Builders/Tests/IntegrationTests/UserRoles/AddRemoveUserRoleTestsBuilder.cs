namespace Craftsman.Builders.Tests.IntegrationTests.UserRoles;

using System;
using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;

public class AddRemoveUserRoleTestsBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public AddRemoveUserRoleTestsBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string testDirectory, string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"AddRemoveUserRoleTests.cs", "Users", projectBaseName);
        var fileText = WriteTestFileText(testDirectory, srcDirectory, classPath, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string testDirectory, string srcDirectory, ClassPath classPath, string projectBaseName)
    {
        var testFixtureName = FileNames.GetIntegrationTestFixtureName();
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", "User", projectBaseName);
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, "", "Users", projectBaseName);
        var rolesClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", "Roles", projectBaseName);


        return @$"namespace {classPath.ClassNamespace};

using {rolesClassPath.ClassNamespace};
using {fakerClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Threading.Tasks;

public class {classPath.ClassNameWithoutExt} : TestBase
{{    
    [Fact]
    public async Task can_add_and_remove_role()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        var faker = new Faker();
        var fakeUserOne = new FakeUserBuilder().Build();
        await testingServiceScope.InsertAsync(fakeUserOne);

        var user = await testingServiceScope.ExecuteDbContextAsync(db => db.Users
            .FirstOrDefaultAsync(u => u.Id == fakeUserOne.Id));
        var id = user.Id;
        var role = faker.PickRandom<RoleEnum>(RoleEnum.List).Name;

        // Act - Add
        var command = new AddUserRole.Command(id, role);
        await testingServiceScope.SendAsync(command);
        var updatedUser = await testingServiceScope.ExecuteDbContextAsync(db => db.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == id));

        // Assert - Add
        updatedUser.Roles.Count.Should().Be(1);
        updatedUser.Roles.FirstOrDefault().Role.Value.Should().Be(role);
        
        // Act - Remove
        var removeCommand = new RemoveUserRole.Command(id, role);
        await testingServiceScope.SendAsync(removeCommand);
        
        // Assert - Remove
        updatedUser = await testingServiceScope.ExecuteDbContextAsync(db => db.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == id));
        updatedUser.Roles.Count.Should().Be(0);
    }}
}}";
    }
}
