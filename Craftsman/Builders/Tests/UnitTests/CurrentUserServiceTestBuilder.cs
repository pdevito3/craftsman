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

    public void CreateTests(string testDirectory, string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.UnitTestServiceTestsClassPath(testDirectory, $"CurrentUserServiceTests.cs", projectBaseName);
        var fileText = WriteTestFileText(srcDirectory, classPath, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string srcDirectory, ClassPath classPath, string projectBaseName)
    {
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        var hangfireUtilsClassPath = ClassPathHelper.HangfireResourcesClassPath(srcDirectory, $"", projectBaseName);

        return @$"namespace {classPath.ClassNamespace};

using {servicesClassPath.ClassNamespace};
using {hangfireUtilsClassPath.ClassNamespace};
using System.Security.Claims;
using Bogus;
using Microsoft.AspNetCore.Http;
using NSubstitute;

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
        
        var currentUserService = new CurrentUserService(sub, null);

        currentUserService.UserId.Should().Be(name);
    }}
    
    [Fact]
    public void can_fallback_to_user_in_job_context()
    {{
        // Arrange
        var name = new Faker().Person.UserName;

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns((HttpContext)null);

        var jobContextAccessor = new JobContextAccessor();
        jobContextAccessor.UserContext = new JobWithUserContext()
        {{
            User = name
        }};

        var currentUserService = new CurrentUserService(httpContextAccessor, jobContextAccessor);

        // Act & Assert
        currentUserService.UserId.Should().Be(name);
    }}
    
    [Fact]
    public void returns_null_if_user_is_not_present()
    {{
        var context = new DefaultHttpContext().HttpContext;
        var sub = Substitute.For<IHttpContextAccessor>();
        sub.HttpContext.Returns(context);
        
        var currentUserService = new CurrentUserService(sub, null);

        currentUserService.UserId.Should().BeNullOrEmpty();
    }}
}}";
    }
}
