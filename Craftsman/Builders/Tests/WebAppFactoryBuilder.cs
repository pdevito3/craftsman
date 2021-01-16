namespace Craftsman.Builders.Tests.IntegrationTests
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class WebAppFactoryBuilder
    {
        public static void CreateWebAppFactory(string solutionDirectory, string solutionName, string dbContextName)
        {
            try
            {
                var classPath = ClassPathHelper.TestProjectRootClassPath(solutionDirectory, $"CustomWebApplicationFactory.cs", solutionName);

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    File.Delete(classPath.FullClassPath); // saves me from having to make a remover!

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = GetWebAppFactoryFileText(classPath, dbContextName);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        private static string GetWebAppFactoryFileText(ClassPath classPath, string dbContextName)
        {
            return @$"
namespace {classPath.ClassNamespace}
{{
    using Infrastructure.Persistence.Contexts;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Respawn;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using WebApi;

    public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : WebApplicationFactory<Startup>
    {{
        // checkpoint for respawn to clear the database when spenning up each time
        private static Checkpoint checkpoint = new Checkpoint
        {{
            
        }};

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {{
            builder.UseEnvironment(""Development"");

            builder.ConfigureServices(async services =>
            {{
                services.AddEntityFrameworkInMemoryDatabase();

                // Create a new service provider.
                var provider = services
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                // Add a database context ({dbContextName}) using an in-memory 
                // database for testing.
                services.AddDbContext<{dbContextName}>(options =>
                {{
                    options.UseInMemoryDatabase(""InMemoryDbForTesting"");
                    options.UseInternalServiceProvider(provider);
                }});

                // Build the service provider.
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database
                // context (ApplicationDbContext).
                using (var scope = sp.CreateScope())
                {{
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<{dbContextName}>();

                    // Ensure the database is created.
                    db.Database.EnsureCreated();

                    try
                    {{
                        await checkpoint.Reset(db.Database.GetDbConnection());
                    }}
                    catch
                    {{
                    }}
                }}
            }});
        }}

        public HttpClient GetAnonymousClient()
        {{
            return CreateClient();
        }}
    }}
}}";
        }
    }
}
