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
        var equivalency = provider == DbProvider.Postgres
            ? $@"

        // close to equivalency required to reconcile precision differences between EF and Postgres
        AssertionOptions.AssertEquivalencyUsing(options => 
        {{
            options.Using<DateTime>(ctx => ctx.Subject
                .Should()
                .BeCloseTo(ctx.Expectation, 1.Seconds())).WhenTypeIs<DateTime>();
            options.Using<DateTimeOffset>(ctx => ctx.Subject
                .Should()
                .BeCloseTo(ctx.Expectation, 1.Seconds())).WhenTypeIs<DateTimeOffset>();

            return options;
        }});"
            : null;

        return @$"namespace {classNamespace};

using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using AutoBogus;
using FluentAssertions.Extensions;
using static {testFixtureName};

public class TestBase
{{
    [SetUp]
    public Task TestSetUp()
    {{{equivalency}
    
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
