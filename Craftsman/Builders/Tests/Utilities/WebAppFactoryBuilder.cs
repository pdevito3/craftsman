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

    public void CreateWebAppFactory(string solutionDirectory, string projectName, string dbContextName, bool addJwtAuthentication)
    {
        var classPath = ClassPathHelper.FunctionalTestProjectRootClassPath(solutionDirectory, $"{FileNames.GetWebHostFactoryName()}.cs", projectName);
        var fileText = GetWebAppFactoryFileText(classPath, dbContextName, solutionDirectory, projectName, addJwtAuthentication);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetWebAppFactoryFileText(ClassPath classPath, string dbContextName, string solutionDirectory, string projectBaseName, bool addJwtAuthentication)
    {
        var webApiClassPath = ClassPathHelper.WebApiProjectRootClassPath(solutionDirectory, "", projectBaseName);
        var contextClassPath = ClassPathHelper.DbContextClassPath(solutionDirectory, "", projectBaseName);
        var utilsClassPath = ClassPathHelper.WebApiResourcesClassPath(solutionDirectory, "", projectBaseName);

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

using {contextClassPath.ClassNamespace};
using {utilsClassPath.ClassNamespace};
using {webApiClassPath.ClassNamespace};{authUsing}
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : WebApplicationFactory<Program>
{{
    protected override IHost CreateHost(IHostBuilder builder)
    {{
        builder.UseEnvironment(Consts.Testing.FunctionalTestingEnvName);

        builder.ConfigureServices(services =>
        {{{authRegistration}
            var provider = services.BuildServiceProvider();

            var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
            typeAdapterConfig.Scan(Assembly.GetExecutingAssembly());
            var mapperConfig = new Mapper(typeAdapterConfig);
            services.AddSingleton<IMapper>(mapperConfig);

            services.AddDbContext<{dbContextName}>(options =>
            {{
                options.UseInMemoryDatabase(""InMemoryDbForTesting"");
                options.UseInternalServiceProvider(provider);
            }});

            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<{dbContextName}>();
            db.Database.EnsureCreated();
        }});
        
        return base.CreateHost(builder);
    }}
}}";
    }
}
