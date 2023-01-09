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

    public void CreateBase(string solutionDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.IntegrationTestProjectRootClassPath(solutionDirectory, "TestBase.cs", projectBaseName);
        var fileText = GetBaseText(classPath.ClassNamespace);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetBaseText(string classNamespace)
    {
        return @$"namespace {classNamespace};

using NUnit.Framework;
using AutoBogus;

[Parallelizable]
public class TestBase
{{
    [SetUp]
    public Task TestSetUp()
    {{
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
