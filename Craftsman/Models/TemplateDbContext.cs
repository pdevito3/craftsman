namespace Craftsman.Models
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
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

        /// <summary>
        /// The database provider for this dbcontext. You can choose SqlServer, MySql, or Postgres
        /// </summary>
        public string Provider
        {
            get => Enum.GetName(typeof(DbProvider), _provider);
            set
            {
                if (!Enum.TryParse<DbProvider>(value, true, out var parsed))
                {
                    throw new InvalidDbProviderException(value);
                }
                _provider = parsed;
            }
        }
    }
}
