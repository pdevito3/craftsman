namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System;
    using System.IO.Abstractions;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class TestFixtureBuilder
    {
        public static void CreateFixture(string solutionDirectory, string projectBaseName, string dbContextName, string provider, IFileSystem fileSystem)
        {
            try
            {
                var classPath = ClassPathHelper.IntegrationTestProjectRootClassPath(solutionDirectory, "TestFixture.cs", projectBaseName);

                if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                    fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

                if (fileSystem.File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (var fs = fileSystem.File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetFixtureText(classPath.ClassNamespace, solutionDirectory, projectBaseName, dbContextName, provider);
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

        public static string GetFixtureText(string classNamespace, string solutionDirectory, string projectBaseName, string dbContextName, string provider)
        {
            var apiClassPath = ClassPathHelper.WebApiProjectClassPath(solutionDirectory, projectBaseName);
            var contextClassPath = ClassPathHelper.DbContextClassPath(solutionDirectory, "", projectBaseName);
            var testUtilsClassPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(solutionDirectory, projectBaseName, "");

            var usingStatement = Enum.GetName(typeof(DbProvider), DbProvider.Postgres) == provider
                ? $@"
    using Npgsql;"
                : null;

            var checkpoint = Enum.GetName(typeof(DbProvider), DbProvider.Postgres) == provider
                ? $@"_checkpoint = new Checkpoint
            {{
                TablesToIgnore = new[] {{ ""__EFMigrationsHistory"" }},
                SchemasToExclude = new[] {{ ""information_schema"", ""pg_subscription"", ""pg_catalog"", ""pg_toast"" }},
                DbAdapter = DbAdapter.Postgres
            }};"
                : $@"_checkpoint = new Checkpoint
            {{
                TablesToIgnore = new[] {{ ""__EFMigrationsHistory"" }},
            }};";

            var resetString = Enum.GetName(typeof(DbProvider), DbProvider.Postgres) == provider
                ? $@"using (var conn = new NpgsqlConnection(_configuration.GetConnectionString(""{dbContextName}"")))
            {{
                await conn.OpenAsync();
                await _checkpoint.Reset(conn);
            }}"
                : $@"await _checkpoint.Reset(_configuration.GetConnectionString(""{dbContextName}""));";

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
    using Moq;{usingStatement}
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
                        {{ ""ConnectionStrings:{dbContextName}"", dockerConnectionString }}
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

            {checkpoint}

            EnsureDatabase();
        }}

        private static void EnsureDatabase()
        {{
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetService<{dbContextName}>();
            
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
            await _checkpoint.Reset(_configuration.GetConnectionString(""{dbContextName}""));
            //_currentUserId = null;

            //using (var conn = new NpgsqlConnection(_configuration.GetConnectionString(""{dbContextName}"")))
            //{{
            //    await conn.OpenAsync();

            //    await _checkpoint.Reset(conn);
            //}}
        }}

        public static async Task<TEntity> FindAsync<TEntity>(params object[] keyValues)
            where TEntity : class
        {{
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetService<{dbContextName}>();

            return await context.FindAsync<TEntity>(keyValues);
        }}

        public static async Task AddAsync<TEntity>(TEntity entity)
            where TEntity : class
        {{
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetService<{dbContextName}>();

            context.Add(entity);

            await context.SaveChangesAsync();
        }}

        public static async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
        {{
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<{dbContextName}>();

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
            var dbContext = scope.ServiceProvider.GetRequiredService<{dbContextName}>();

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

        public static Task ExecuteDbContextAsync(Func<{dbContextName}, Task> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<{dbContextName}>()));

        public static Task ExecuteDbContextAsync(Func<{dbContextName}, ValueTask> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<{dbContextName}>()).AsTask());

        public static Task ExecuteDbContextAsync(Func<{dbContextName}, IMediator, Task> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<{dbContextName}>(), sp.GetService<IMediator>()));

        public static Task<T> ExecuteDbContextAsync<T>(Func<{dbContextName}, Task<T>> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<{dbContextName}>()));

        public static Task<T> ExecuteDbContextAsync<T>(Func<{dbContextName}, ValueTask<T>> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<{dbContextName}>()).AsTask());

        public static Task<T> ExecuteDbContextAsync<T>(Func<{dbContextName}, IMediator, Task<T>> action)
            => ExecuteScopeAsync(sp => action(sp.GetService<{dbContextName}>(), sp.GetService<IMediator>()));

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