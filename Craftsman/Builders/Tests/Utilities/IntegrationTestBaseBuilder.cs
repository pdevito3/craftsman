namespace Craftsman.Builders.Tests.Utilities;

using Domain;
using Helpers;
using Services;

public class IntegrationTestBaseBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public IntegrationTestBaseBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateBase(string solutionDirectory, string projectBaseName, DbProvider provider)
    {
        var classPath = ClassPathHelper.IntegrationTestProjectRootClassPath(solutionDirectory, "TestBase.cs", projectBaseName);
        var fileText = GetBaseText(classPath.ClassNamespace, provider);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetBaseText(string classNamespace, DbProvider provider)
    {
        var testFixtureName = FileNames.GetIntegrationTestFixtureName();

        return @$"namespace {classNamespace};

using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using AutoBogus;
using static {testFixtureName};

[Parallelizable]
public class TestBase
{{
    [SetUp]
    public Task TestSetUp()
    {{    
        AutoFaker.Configure(builder =>
        {{
            // configure global autobogus settings here
            builder.WithRecursiveDepth(1)
                .WithTreeDepth(1)
                .WithRepeatCount(1);
        }});
        
        return Task.CompletedTask;
    }}
}}";
    }
}
