namespace Craftsman.Domain;

using Ardalis.SmartEnum;

public abstract class DbProvider : SmartEnum<DbProvider>
{
    public static readonly DbProvider Postgres = new PostgresType();
    public static readonly DbProvider SqlServer = new SqlServerType();
    public static readonly DbProvider MySql = new MySqlType();

    protected DbProvider(string name, int value) : base(name, value)
    {
    }

    public abstract string PackageInclusionString(string version);
    public abstract string OTelSource();
    public abstract string TestingDbSetupMethod(string projectBaseName, bool isIntegrationTesting);
    public abstract string IntegrationTestConnectionStringSetup();
    public abstract string ResetString();
    public abstract int Port();
    public abstract string DbConnectionStringCompose(string dbHostName, string dbName, string dbUser, string dbPassword);
    public abstract string DbConnectionString(string dbHostName, int? dbPort, string dbName, string dbUser, string dbPassword);

    private class PostgresType : DbProvider
    {
        public PostgresType() : base(nameof(Postgres), 1) { }
        public override string PackageInclusionString(string version)
            => @$"<PackageReference Include=""Npgsql.EntityFrameworkCore.PostgreSQL"" Version=""{version}"" />";
        public override string OTelSource()
            => @$"Npgsql";

        public override string TestingDbSetupMethod(string projectBaseName, bool isIntegrationTesting)
        {
            var testName = isIntegrationTesting ? "IntegrationTesting" : "FunctionalTesting";
            return $@"private static TestcontainerDatabase dbSetup()
    {{
        return new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {{
                Database = ""db"",
                Username = ""postgres"",
                Password = ""postgres""
            }})
            .WithName($""{testName}_{projectBaseName}_{{Guid.NewGuid()}}"")
            .WithImage(""postgres:latest"")
            .Build();
    }}";
        }

        public override string IntegrationTestConnectionStringSetup() 
            => $@"Environment.SetEnvironmentVariable(""DB_CONNECTION_STRING"", _dbContainer.ConnectionString);";

        public override string ResetString()
        {
            return $@"await using var connection = new NpgsqlConnection(EnvironmentService.DbConnectionString);
        await connection.OpenAsync();
        try
        {{
            var respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {{
                TablesToIgnore = new Table[] {{ ""__EFMigrationsHistory"" }},
                SchemasToExclude = new[] {{ ""information_schema"", ""pg_subscription"", ""pg_catalog"", ""pg_toast"" }},
                DbAdapter = DbAdapter.Postgres
            }});
            await respawner.ResetAsync(connection);
        }}
        catch (InvalidOperationException e)
        {{
            throw new Exception($""There was an issue resetting your database state. You might need to add a migration to your project. You can add a migration with `dotnet ef migration add YourMigrationDescription`. More details on this error: {{e.Message}}"");
        }}";
        }

        public override int Port()
            => 5432;
        public override string DbConnectionStringCompose(string dbHostName, string dbName, string dbUser,
            string dbPassword)
            => $"Host={dbHostName};Port={5432};Database={dbName};Username={dbUser};Password={dbPassword}";
        public override string DbConnectionString(string dbHostName, int? dbPort, string dbName, string dbUser, string dbPassword)
             => $"Host=localhost;Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";
    }

    private class SqlServerType : DbProvider
    {
        public SqlServerType() : base(nameof(SqlServer), 2) { }
        public override string PackageInclusionString(string version) 
            => @$"<PackageReference Include=""Microsoft.EntityFrameworkCore.SqlServer"" Version=""{version}"" />
    <PackageReference Include = ""Microsoft.EntityFrameworkCore.Tools"" Version = ""{version}"" /> ";
        
        public override string OTelSource()
            => @$"Microsoft.EntityFrameworkCore.SqlServer";

        public override string TestingDbSetupMethod(string projectBaseName, bool isIntegrationTesting)
        {
            var testName = isIntegrationTesting ? "IntegrationTesting" : "FunctionalTesting";
            return $@"private static TestcontainerDatabase dbSetup()
    {{
        var isMacOs = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        var cpuArch = RuntimeInformation.ProcessArchitecture;
        var isRunningOnMacOsArm64 = isMacOs && cpuArch == Architecture.Arm64;
        
        var baseDb = new TestcontainersBuilder<MsSqlTestcontainer>()
            .WithDatabase(new MsSqlTestcontainerConfiguration()
            {{
                Password = ""#testingDockerPassword#"",
            }})
            .WithName($""{testName}_{projectBaseName}_{Guid.NewGuid()}"");
            
        if(isRunningOnMacOsArm64)
            baseDb.WithImage(""mcr.microsoft.com/azure-sql-edge:latest"");

        return baseDb.Build();
    }}";
        }

        public override string IntegrationTestConnectionStringSetup() 
            => $@"Environment.SetEnvironmentVariable(""DB_CONNECTION_STRING"", $""{{_dbContainer.ConnectionString}}TrustServerCertificate=true;"");";
        public override string ResetString()
            => $@"try
        {{
            var respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {{
                TablesToIgnore = new Table[] {{ ""__EFMigrationsHistory"" }},
            }});
            await respawner.ResetAsync(EnvironmentService.DbConnectionString);
        }}
        catch (InvalidOperationException e)
        {{
            throw new Exception($""There was an issue resetting your database state. You might need to add a migration to your project. You can add a migration with `dotnet ef migration add YourMigrationDescription`. More details on this error: {{e.Message}}"");
        }}";

        public override int Port()
            => 1433;
        public override string DbConnectionStringCompose(string dbHostName, string dbName, string dbUser, string dbPassword)
            => $"Data Source={dbHostName},{1433};Integrated Security=False;Database={dbName};User ID={dbUser};Password={dbPassword}";
        public override string DbConnectionString(string dbHostName, int? dbPort, string dbName, string dbUser, string dbPassword)
            => $"Data Source=localhost,{dbPort};TrustServerCertificate=True;Integrated Security=False;Database={dbName};User ID={dbUser};Password={dbPassword}";
    }

    private class MySqlType : DbProvider
    {
        private const string Response = "MySql is not supported";
        public MySqlType() : base(nameof(MySql), 3) { }
        public override string PackageInclusionString(string version)
            => throw new Exception(Response);
        public override string OTelSource()
            => throw new Exception(Response);
        public override string TestingDbSetupMethod(string projectBaseName, bool isIntegrationTesting)
            => throw new Exception(Response);
        public override string IntegrationTestConnectionStringSetup()
            => throw new Exception(Response);
        public override string ResetString() 
            => throw new Exception(Response);
        public override int Port()
            => throw new Exception(Response);
        public override string DbConnectionStringCompose(string dbHostName, string dbName, string dbUser, string dbPassword)
            => throw new Exception(Response);
        public override string DbConnectionString(string dbHostName, int? dbPort, string dbName, string dbUser, string dbPassword)
             => throw new Exception(Response);
    }
}
