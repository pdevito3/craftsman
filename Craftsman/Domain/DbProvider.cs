namespace Craftsman.Domain;

using Ardalis.SmartEnum;
using Services;

public abstract class DbProvider : SmartEnum<DbProvider>
{
    public static readonly DbProvider Postgres = new PostgresType();
    public static readonly DbProvider SqlServer = new SqlServerType();
    public static readonly DbProvider MySql = new MySqlType();

    protected DbProvider(string name, int value) : base(name, value)
    {
    }

    public abstract string ApiPackageInclusionString(string version);
    public abstract string OTelSource();
    public abstract string TestingDbSetupUsings();
    public abstract string TestingContainerDb();
    public abstract string TestingDbSetupMethod(string projectBaseName, bool isIntegrationTesting);
    public abstract string IntegrationTestConnectionStringSetup(string configKeyName);
    public abstract string DbRegistrationStatement();
    public abstract string TestingCsProjNugetPackages();
    public abstract string DbDisposal();
    public abstract int Port();
    public abstract string DbConnectionStringCompose(string dbHostName, string dbName, string dbUser, string dbPassword);
    public abstract string DbConnectionString(string dbHostName, int? dbPort, string dbName, string dbUser, string dbPassword);

    private class PostgresType : DbProvider
    {
        public PostgresType() : base(nameof(Postgres), 1) { }
        public override string ApiPackageInclusionString(string version)
            => @$"<PackageReference Include=""Npgsql.EntityFrameworkCore.PostgreSQL"" Version=""{version}"" />";
        public override string OTelSource()
            => @$"Npgsql";
        public override string DbRegistrationStatement() => @$"UseNpgsql";
        public override string TestingCsProjNugetPackages() => @$"
    <PackageReference Include=""Testcontainers.PostgreSql"" Version=""3.3.0"" />";
        public override string DbDisposal() => @$"await _dbContainer.DisposeAsync();";
        public override string TestingContainerDb() => @$"
    private PostgreSqlContainer _dbContainer;";
        public override string TestingDbSetupUsings() => $@"{Environment.NewLine}using Testcontainers.PostgreSql;";
        public override string TestingDbSetupMethod(string projectBaseName, bool isIntegrationTesting)
        {
            var migrations = isIntegrationTesting
                ? $@"{Environment.NewLine}        await RunMigration(_dbContainer.GetConnectionString());"
                : null;
            var connectionStringAssignment = isIntegrationTesting 
                ? IntegrationTestConnectionStringSetup(FileNames.ConnectionStringOptionKey(projectBaseName)) 
                : $@"Environment.SetEnvironmentVariable($""{{ConnectionStringOptions.SectionName}}__{{ConnectionStringOptions.{FileNames.ConnectionStringOptionKey(projectBaseName)}}}"", _dbContainer.GetConnectionString());";
            return $@"_dbContainer = new PostgreSqlBuilder().Build();
        await _dbContainer.StartAsync();
        {connectionStringAssignment}{migrations}";
        }

        public override string IntegrationTestConnectionStringSetup(string configKeyName) 
            => $@"builder.Configuration.GetSection(ConnectionStringOptions.SectionName)[ConnectionStringOptions.{configKeyName}] = _dbContainer.GetConnectionString();";

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
        public override string ApiPackageInclusionString(string version) 
            => @$"<PackageReference Include=""Microsoft.EntityFrameworkCore.SqlServer"" Version=""{version}"" />
    <PackageReference Include = ""Microsoft.EntityFrameworkCore.Tools"" Version = ""{version}"" /> ";
        
        public override string OTelSource()
            => @$"Microsoft.EntityFrameworkCore.SqlServer";        
        public override string DbRegistrationStatement() => @$"UseSqlServer";
        public override string TestingCsProjNugetPackages() => @$"
    <PackageReference Include=""Testcontainers.SqlEdge"" Version=""3.3.0"" />
    <PackageReference Include=""Testcontainers.MsSql"" Version=""3.3.0"" />";
        public override string DbDisposal() => @$"try
        {{
            await _msSqlContainer.DisposeAsync();
        }}
        catch {{ /* ignore*/ }}

        try
        {{
            await _edgeContainer.DisposeAsync();
        }}
        catch {{ /* ignore*/ }}";
        public override string TestingContainerDb() => @$"
    private SqlEdgeContainer _edgeContainer;
    private MsSqlContainer _msSqlContainer;";

        public override string TestingDbSetupUsings() => @$"{Environment.NewLine}using System.Runtime.InteropServices;
using Testcontainers.MsSql;
using Testcontainers.SqlEdge;";
        public override string TestingDbSetupMethod(string projectBaseName, bool isIntegrationTesting)
        {
            var migrations = isIntegrationTesting 
                ? $@"{Environment.NewLine}        await RunMigration(connection);"
                : "";
            var connectionStringAssignment = isIntegrationTesting 
                ? IntegrationTestConnectionStringSetup(FileNames.ConnectionStringOptionKey(projectBaseName)) 
                : $@"Environment.SetEnvironmentVariable($""{{ConnectionStringOptions.SectionName}}__{{ConnectionStringOptions.{FileNames.ConnectionStringOptionKey(projectBaseName)}}}"", connection);";
            return $@"var isMacOs = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        var cpuArch = RuntimeInformation.ProcessArchitecture;
        var isRunningOnMacOsArm64 = isMacOs && cpuArch == Architecture.Arm64;
        string connection;

        if (isRunningOnMacOsArm64)
        {{
            _edgeContainer = new SqlEdgeBuilder().Build();
            await _edgeContainer.StartAsync();
            connection = _edgeContainer.GetConnectionString();
        }}
        else
        {{
            _msSqlContainer = new MsSqlBuilder().Build();
            await _msSqlContainer.StartAsync();
            connection = _msSqlContainer.GetConnectionString();
        }}
        
        {connectionStringAssignment}{migrations}";
        }

        public override string IntegrationTestConnectionStringSetup(string configKeyName) 
            => $@"builder.Configuration.GetSection(ConnectionStringOptions.SectionName)[ConnectionStringOptions.{configKeyName}] = connection;";
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
        public override string ApiPackageInclusionString(string version)
            => throw new Exception(Response);
        public override string OTelSource()
            => throw new Exception(Response);
        public override string DbRegistrationStatement()
            => throw new Exception(Response);
        public override string DbDisposal()
            => throw new Exception(Response);
        public override string TestingContainerDb()
            => throw new Exception(Response);
        public override string TestingDbSetupUsings() => null;
        public override string TestingDbSetupMethod(string projectBaseName, bool isIntegrationTesting)
            => throw new Exception(Response);
        public override string IntegrationTestConnectionStringSetup(string configKeyName)
            => throw new Exception(Response);
        public override int Port()
            => throw new Exception(Response);
        public override string DbConnectionStringCompose(string dbHostName, string dbName, string dbUser, string dbPassword)
            => throw new Exception(Response);
        public override string DbConnectionString(string dbHostName, int? dbPort, string dbName, string dbUser, string dbPassword)
             => throw new Exception(Response);
        public override string TestingCsProjNugetPackages()
            => throw new Exception(Response);
    }
}
