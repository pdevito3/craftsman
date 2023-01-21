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
    public abstract string DbRegistrationStatement();
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
        public override string DbRegistrationStatement() => @$"UseNpgsql";

        public override string TestingDbSetupMethod(string projectBaseName, bool isIntegrationTesting)
        {
            var testName = isIntegrationTesting ? "IntegrationTesting" : "FunctionalTesting";
            return $@"private static TestcontainerDatabase DbSetup()
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
            => $@"Environment.SetEnvironmentVariable(EnvironmentService.DbConnectionStringKey, _dbContainer.ConnectionString);";

        public override int Port() => 5432;
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
        public override string DbRegistrationStatement() => @$"UseSqlServer";

        public override string TestingDbSetupMethod(string projectBaseName, bool isIntegrationTesting)
        {
            var testName = isIntegrationTesting ? "IntegrationTesting" : "FunctionalTesting";
            return $@"private static TestcontainerDatabase DbSetup()
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
            => $@"Environment.SetEnvironmentVariable(EnvironmentService.DbConnectionStringKey, $""{{_dbContainer.ConnectionString}}TrustServerCertificate=true;"");";
        public override int Port() => 1433;
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
        public override string DbRegistrationStatement()
            => throw new Exception(Response);
        public override string TestingDbSetupMethod(string projectBaseName, bool isIntegrationTesting)
            => throw new Exception(Response);
        public override string IntegrationTestConnectionStringSetup()
            => throw new Exception(Response);
        public override int Port()
            => throw new Exception(Response);
        public override string DbConnectionStringCompose(string dbHostName, string dbName, string dbUser, string dbPassword)
            => throw new Exception(Response);
        public override string DbConnectionString(string dbHostName, int? dbPort, string dbName, string dbUser, string dbPassword)
             => throw new Exception(Response);
    }
}
