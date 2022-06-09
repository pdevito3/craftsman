namespace Craftsman.Builders.Tests.UnitTests;

using System.IO;
using Helpers;
using Services;

public class CurrentUserServiceTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public CurrentUserServiceTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.UnitTestWrapperTestsClassPath(solutionDirectory, $"CurrentUserServiceTests.cs", projectBaseName);
        var fileText = WriteTestFileText(solutionDirectory, classPath, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string solutionDirectory, ClassPath classPath, string projectBaseName)
    {
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(solutionDirectory, "", projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {servicesClassPath.ClassNamespace};
using System.Security.Claims;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}
{{
    [Fact]
    public void returns_user_in_context_if_present()
    {{
        var name = new Faker().Person.UserName;

        var id = new ClaimsIdentity();
        id.AddClaim(new Claim(ClaimTypes.NameIdentifier, name));

        var context = new DefaultHttpContext().HttpContext;
        context.User = new ClaimsPrincipal(id);

        var sub = Substitute.For<IHttpContextAccessor>();
        sub.HttpContext.Returns(context);
        
        var currentUserService = new CurrentUserService(sub);

        currentUserService.UserId.Should().Be(name);
    }}
    
    [Fact]
    public void returns_null_if_user_is_not_present()
    {{
        var context = new DefaultHttpContext().HttpContext;
        var sub = Substitute.For<IHttpContextAccessor>();
        sub.HttpContext.Returns(context);
        
        var currentUserService = new CurrentUserService(sub);

        currentUserService.UserId.Should().BeNullOrEmpty();
    }}
}}";
    }
}
