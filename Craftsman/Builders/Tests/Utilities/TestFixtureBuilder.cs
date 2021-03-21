namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System;
    using System.IO.Abstractions;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class TestFixtureBuilder
    {
        public static void CreateFixture(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            try
            {
                var classPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(solutionDirectory, projectBaseName, "TestFixture.cs");

                if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                    fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

                if (fileSystem.File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (var fs = fileSystem.File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetFixtureText(classPath.ClassNamespace, solutionDirectory, projectBaseName);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{fileSystem.Path.DirectorySeparatorChar}", ""));
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

        public static string GetFixtureText(string classNamespace, string solutionDirectory, string projectBaseName)
        {
            var apiClassPath = ClassPathHelper.WebApiProjectClassPath(solutionDirectory, projectBaseName);
            var contextClassPath = ClassPathHelper.DbContextClassPath(solutionDirectory, "", projectBaseName);
            var testUtilsClassPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(solutionDirectory, projectBaseName, "");

            return @$"namespace {classNamespace}
{{
    using {contextClassPath.ClassNamespace};
    using {testUtilsClassPath.ClassNamespace};
    using {apiClassPath.ClassNamespace};
    using MediatR;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Npgsql;
    using NUnit.Framework;
    using Respawn;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    [SetUpFixture]
    public class TestFixture
    {{
        private static IConfigurationRoot _configuration;
        private static IWebHostEnvironment _env;
        private static IServiceScopeFactory _scopeFactory;
        private static Checkpoint _checkpoint;
        //private static string _currentUserId;

        public const string DATABASE_NAME_PLACEHOLDER = ""@@databaseName@@"";
        private string _dockerContainerId;
        private string _dockerSqlPort;

        [OneTimeSetUp]
        public async Task RunBeforeAnyTests()
        {{

            (_dockerContainerId, _dockerSqlPort) = await DockerSqlDatabaseUtilities.EnsureDockerStartedAndGetContainerIdAndPortAsync();

            var dockerConnectionString = GetSqlConnectionString();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddInMemoryCollection(new Dictionary<string, string>
                    {{
                        {{ ""UseInMemoryDatabase"", ""false"" }},
                        {{ ""ConnectionStrings:AccessioningDbContext"", dockerConnectionString }}
                    }})
                .AddEnvironmentVariables();

            _configuration = builder.Build();
            _env = Mock.Of<IWebHostEnvironment>();

            var startup = new Startup(_configuration, _env);

            var services = new ServiceCollection();

            services.AddLogging();

            startup.ConfigureServices(services);

            // Replace service registration for ICurrentUserService
            // Remove existing registration
            //var currentUserServiceDescriptor = services.FirstOrDefault(d =>
            //    d.ServiceType == typeof(ICurrentUserService));

            //services.Remove(currentUserServiceDescriptor);

            // Register testing version
            //services.AddTransient(provider =>
            //    Mock.Of<ICurrentUserService>(s => s.UserId == _currentUserId));

            _scopeFactory = services.BuildServiceProvider().GetService<IServiceScopeFactory>();

            _checkpoint = new Checkpoint
            {{
                TablesToIgnore = new[] {{ ""__EFMigrationsHistory"" }},
                //SchemasToExclude = new[] {{ ""information_schema"", ""pg_subscription"", ""pg_catalog"", ""pg_toast"" }},
                //DbAdapter = DbAdapter.Postgres
            }};

            EnsureDatabase();
        }}

        private static void EnsureDatabase()
        {{
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetService<AccessioningDbContext>();
            
            context.Database.Migrate();
        }}

        public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        {{
            using var scope = _scopeFactory.CreateScope();

            var mediator = scope.ServiceProvider.GetService<ISender>();

            return await mediator.Send(request);
        }}

        public static async Task ResetState()
        {{
            await _checkpoint.Reset(_configuration.GetConnectionString(""AccessioningDbContext""));
            //_currentUserId = null;

            //using (var conn = new NpgsqlConnection(_configuration.GetConnectionString(""AccessioningDbContext"")))
            //{{
            //    await conn.OpenAsync();

            //    await _checkpoint.Reset(conn);
            //}}
        }}

        public static async Task<TEntity> FindAsync<TEntity>(params object[] keyValues)
            where TEntity : class
        {{
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetService<AccessioningDbContext>();

            return await context.FindAsync<TEntity>(keyValues);
        }}

        public static async Task AddAsync<TEntity>(TEntity entity)
            where TEntity : class
        {{
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetService<AccessioningDbContext>();

            context.Add(entity);

            await context.SaveChangesAsync();
        }}

        public static async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
        {{
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AccessioningDbContext>();

            try
            {{
                //await dbContext.BeginTransactionAsync();

                await action(scope.ServiceProvider);

                //await dbContext.CommitTransactionAsync();
            }}
            catch (Exception)
            {{
                //dbContext.RollbackTransaction();
                throw;
            }}
        }}

        public static async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
        {{
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AccessioningDbContext>();

            try
            {{
                //await dbContext.BeginTransactionAsync();

                var result = await action(scope.ServiceProvider);

                //await dbContext.CommitTransactionAsync();

                return result;
            }}
            catch (Exception)
            {{
                //dbContext.RollbackTransaction();
                throw;
            }}
        }}

        public static Task ExecuteDbContextAsync(Func<AccessioningDbContext, Task> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<AccessioningDbContext>()));

        public static Task ExecuteDbContextAsync(Func<AccessioningDbContext, ValueTask> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<AccessioningDbContext>()).AsTask());

        public static Task ExecuteDbContextAsync(Func<AccessioningDbContext, IMediator, Task> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<AccessioningDbContext>(), sp.GetService<IMediator>()));

        public static Task<T> ExecuteDbContextAsync<T>(Func<AccessioningDbContext, Task<T>> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<AccessioningDbContext>()));

        public static Task<T> ExecuteDbContextAsync<T>(Func<AccessioningDbContext, ValueTask<T>> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<AccessioningDbContext>()).AsTask());

        public static Task<T> ExecuteDbContextAsync<T>(Func<AccessioningDbContext, IMediator, Task<T>> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<AccessioningDbContext>(), sp.GetService<IMediator>()));

        public static Task<int> InsertAsync<T>(params T[] entities) where T : class
        {{
            return ExecuteDbContextAsync(db =>
            {{
                foreach (var entity in entities)
                {{
                    db.Set<T>().Add(entity);
                }}
                return db.SaveChangesAsync();
            }});
        }}

        public string GetSqlConnectionString()
        {{
            return $""Data Source=localhost,{{_dockerSqlPort}};"" +
                $""Initial Catalog={{DATABASE_NAME_PLACEHOLDER}};"" +
                ""Integrated Security=False;"" +
                ""User ID=SA;"" +
                $""Password={{DockerSqlDatabaseUtilities.SQLSERVER_SA_PASSWORD}}"";
        }}

        [OneTimeTearDown]
        public Task RunAfterAnyTests()
        {{
            return Task.CompletedTask;
            //return DockerSqlDatabaseUtilities.EnsureDockerStoppedAndRemovedAsync(_dockerContainerId);
        }}
    }}
}}";
        }
    }
}