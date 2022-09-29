namespace Craftsman.Builders.Tests.Utilities;

using System.IO;
using Domain;
using Helpers;
using Services;

public class FunctionalFixtureBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public FunctionalFixtureBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateFixture(string testDirectory, string projectBaseName, string dbContextName, DbProvider dbProvider)
    {
        var classPath = ClassPathHelper.FunctionalTestProjectRootClassPath(testDirectory, $"{FileNames.GetFunctionalFixtureName()}.cs", projectBaseName);
        var fileText = GetWebAppFactoryFileText(classPath, projectBaseName, dbContextName, dbProvider);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetWebAppFactoryFileText(ClassPath classPath, string projectBaseName, string dbContextName, DbProvider dbProvider)
    {
        return @$"namespace {classPath.ClassNamespace};

using Databases;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Configurations.Databases;
using DotNet.Testcontainers.Containers.Modules.Abstractions;
using DotNet.Testcontainers.Containers.Modules.Databases;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

[SetUpFixture]
public class FunctionalTestFixture
{{
    public static IServiceScopeFactory ScopeFactory {{ get; private set; }}
    public static WebApplicationFactory<Program> Factory  {{ get; private set; }}
    private readonly TestcontainerDatabase _dbContainer = dbSetup();

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {{
        await _dbContainer.StartAsync();
        Environment.SetEnvironmentVariable(""DB_CONNECTION_STRING"", _dbContainer.ConnectionString);
        
        Factory = new TestingWebApplicationFactory();
        ScopeFactory = Factory.Services.GetRequiredService<IServiceScopeFactory>();

        using var scope = ScopeFactory.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<{dbContextName}>();
        await db.Database.MigrateAsync();
    }}
    
    [OneTimeTearDown]
    public async Task OneTimeTearDown()  
    {{
        await _dbContainer.DisposeAsync();
        await Factory.DisposeAsync();
    }}

    {dbProvider.TestingDbSetupMethod(projectBaseName, false)}
}}";
    }
}
