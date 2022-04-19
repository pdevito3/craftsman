namespace NewCraftsman.Domain
{
    using System;

    public class TemplateDbContext
    {
        private DbProvider _provider = DbProvider.SqlServer;

        /// <summary>
        /// The name of the dbContext
        /// </summary>
        public string ContextName { get; set; }

        /// <summary>
        /// The name of the database
        /// </summary>
        public string DatabaseName { get; set; }
        
        public DbProvider ProviderEnum = DbProvider.SqlServer;
        /// <summary>
        /// The database provider for this dbcontext. You can choose SqlServer, MySql, or Postgres
        /// </summary>
        public string Provider
        {
            get => ProviderEnum.Name;
            set
            {
                if (!DbProvider.TryFromName(value, true, out var parsed))
                {
                    parsed = DbProvider.Postgres;
                }

                ProviderEnum = parsed;
            }
        }

        public NamingConventionEnum NamingConventionEnum { get; set; }
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
}
