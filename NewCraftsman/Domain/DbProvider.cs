namespace NewCraftsman.Domain;

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

    private class PostgresType : DbProvider
    {
        public PostgresType() : base(nameof(Postgres), 1) {}
        public override string PackageInclusionString(string version)
            => @$"<PackageReference Include=""Npgsql.EntityFrameworkCore.PostgreSQL"" Version=""{version}"" />";
        public override string OTelSource()
            => @$"Npgsql";
    }

    private class SqlServerType : DbProvider
    {
        public SqlServerType() : base(nameof(SqlServer), 2) {}
        public override string PackageInclusionString(string version)
            => @$"<PackageReference Include=""Microsoft.EntityFrameworkCore.SqlServer"" Version=""{version}"" />";
        public override string OTelSource()
            => @$"Microsoft.EntityFrameworkCore.SqlServer";
    }

    private class MySqlType : DbProvider
    {
        public MySqlType() : base(nameof(MySql), 3) {}
        public override string PackageInclusionString(string version)
            => throw new Exception("MySql is not supported");
        public override string OTelSource()
            => throw new Exception("MySql is not supported");
    }
}
