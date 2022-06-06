namespace Craftsman.Domain;

public class DbContextConfig
{
    private readonly DbProvider _provider = DbProvider.Postgres;

    /// <summary>
    /// The name of the dbContext
    /// </summary>
    public string ContextName { get; set; }

    /// <summary>
    /// The name of the database
    /// </summary>
    public string DatabaseName { get; set; }

    public DbProvider ProviderEnum = DbProvider.Postgres;
    /// <summary>
    /// The database provider for this dbcontext. You can choose SqlServer, MySql, or Postgres
    /// </summary>
    public string Provider
    {
        get => ProviderEnum.Name;
        set
        {
            if (!DbProvider.TryFromName(value, true, out var parsed))
                parsed = DbProvider.Postgres;

            ProviderEnum = parsed;
        }
    }

    public NamingConventionEnum NamingConventionEnum = NamingConventionEnum.SnakeCase;
    /// <summary>
    /// The naming convention for your database
    /// </summary>
    public string NamingConvention
    {
        get => NamingConventionEnum.Name;
        set
        {
            if (!NamingConventionEnum.TryFromName(value, true, out var parsed))
            {
                parsed = NamingConventionEnum.SnakeCase;
            }

            NamingConventionEnum = parsed;
        }
    }
}
