namespace Craftsman.Builders.Tests.Utilities;

using System.IO;
using Helpers;
using Services;

public class WebAppFactoryBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public WebAppFactoryBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateWebAppFactory(string testDirectory, string projectName, bool addJwtAuthentication)
    {
        var classPath = ClassPathHelper.FunctionalTestProjectRootClassPath(testDirectory, $"{FileNames.GetWebHostFactoryName()}.cs", projectName);
        var fileText = GetWebAppFactoryFileText(classPath, testDirectory, projectName, addJwtAuthentication);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetWebAppFactoryFileText(ClassPath classPath, string testDirectory, string projectBaseName, bool addJwtAuthentication)
    {
        var webApiClassPath = ClassPathHelper.WebApiProjectRootClassPath(testDirectory, "", projectBaseName);
        var utilsClassPath = ClassPathHelper.WebApiResourcesClassPath(testDirectory, "", projectBaseName);

        var authUsing = addJwtAuthentication ? $@"
using WebMotions.Fake.Authentication.JwtBearer;" : "";

        var authRegistration = addJwtAuthentication ? $@"
                // add authentication using a fake jwt bearer
                services.AddAuthentication(options =>
                {{
                    options.DefaultAuthenticateScheme = FakeJwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = FakeJwtBearerDefaults.AuthenticationScheme;
                }}).AddFakeJwtBearer();

" : "";

        return @$"
namespace {classPath.ClassNamespace};

using {utilsClassPath.ClassNamespace};
using {webApiClassPath.ClassNamespace};{authUsing}
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Microsoft.Extensions.Logging;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : WebApplicationFactory<Program>
{{
    protected override IHost CreateHost(IHostBuilder builder)
    {{
        builder.UseEnvironment(Consts.Testing.FunctionalTestingEnvName);
        builder.ConfigureLogging(logging =>
        {{
            logging.ClearProviders();
        }});

        builder.ConfigureServices(services =>
        {{{authRegistration}
            var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
            typeAdapterConfig.Scan(Assembly.GetExecutingAssembly());
            var mapperConfig = new Mapper(typeAdapterConfig);
            services.AddSingleton<IMapper>(mapperConfig);
        }});
        
        return base.CreateHost(builder);
    }}
}}";
    }
}
