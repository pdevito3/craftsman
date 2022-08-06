namespace Craftsman.Builders.Tests.Utilities;

using Helpers;
using Services;

public class IntegrationTestBaseBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public IntegrationTestBaseBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateBase(string solutionDirectory, string projectBaseName, bool isProtected)
    {
        var classPath = ClassPathHelper.IntegrationTestProjectRootClassPath(solutionDirectory, "TestBase.cs", projectBaseName);
        var fileText = GetBaseText(classPath.ClassNamespace, isProtected);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetBaseText(string classNamespace, bool isProtected)
    {
        var testFixtureName = FileNames.GetIntegrationTestFixtureName();
        
        var protectedUsings = isProtected ? @$"{Environment.NewLine}
    using HeimGuard;
    using Moq;" : "";
        var heimGuardMock = isProtected 
            ? $@"{Environment.NewLine}        var userPolicyHandler = GetService<IHeimGuardClient>();
        Mock.Get(userPolicyHandler)
            .Setup(x => x.HasPermissionAsync(It.IsAny<string>()))
            .ReturnsAsync(true);{Environment.NewLine}"
            : null;

        return @$"namespace {classNamespace};

using NUnit.Framework;
using System.Threading.Tasks;
using AutoBogus;{protectedUsings}
using static {testFixtureName};

[Parallelizable]
public class TestBase
{{
    [SetUp]
    public Task TestSetUp()
    {{{heimGuardMock}
        AutoFaker.Configure(builder =>
        {{
            // configure global autobogus settings here
            builder.WithDateTimeKind(DateTimeKind.Utc)
                .WithRecursiveDepth(3)
                .WithTreeDepth(1)
                .WithRepeatCount(1);
        }});
        
        return Task.CompletedTask;
    }}
}}";
    }
}
