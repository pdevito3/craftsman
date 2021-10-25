namespace Craftsman.Builders.Tests.Utilities
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.Text;

    public class WebAppFactoryBuilder
    {
        public static void CreateWebAppFactory(string solutionDirectory, string projectName, string dbContextName, bool addJwtAuthentication)
        {
            var classPath = ClassPathHelper.FunctionalTestProjectRootClassPath(solutionDirectory, $"{Utilities.GetWebHostFactoryName()}.cs", projectName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                File.Delete(classPath.FullClassPath); // saves me from having to make a remover!

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = GetWebAppFactoryFileText(classPath, dbContextName, solutionDirectory, projectName, addJwtAuthentication);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }

        private static string GetWebAppFactoryFileText(ClassPath classPath, string dbContextName, string solutionDirectory, string projectBaseName, bool addJwtAuthentication)
        {
            var webApiClassPath = ClassPathHelper.WebApiProjectRootClassPath(solutionDirectory, "", projectBaseName);
            var contextClassPath = ClassPathHelper.DbContextClassPath(solutionDirectory, "", projectBaseName);
            var utilsClassPath = ClassPathHelper.WebApiUtilsClassPath(solutionDirectory, "", projectBaseName);

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
namespace {classPath.ClassNamespace}
{{
    using {contextClassPath.ClassNamespace};
    using {utilsClassPath.ClassNamespace};
    using {webApiClassPath.ClassNamespace};{authUsing}
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : WebApplicationFactory<Startup>
    {{
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {{
            builder.UseEnvironment(LocalConfig.FunctionalTestingEnvName);

            builder.ConfigureServices(services =>
            {{{authRegistration}
                // Create a new service provider.
                var provider = services.BuildServiceProvider();

                // Add a database context ({dbContextName}) using an in-memory database for testing.
                services.AddDbContext<{dbContextName}>(options =>
                {{
                    options.UseInMemoryDatabase(""InMemoryDbForTesting"");
                    options.UseInternalServiceProvider(provider);
                }});

                // Build the service provider.
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context ({dbContextName}).
                using (var scope = sp.CreateScope())
                {{
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<{dbContextName}>();

                    // Ensure the database is created.
                    db.Database.EnsureCreated();
                }}
            }});
        }}
    }}
}}";
        }
    }
}