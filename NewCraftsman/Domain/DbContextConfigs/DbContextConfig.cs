namespace NewCraftsman.Domain.DbContextConfigs;

using AutoMapper;
using Dtos;
using FluentValidation;
using Mappings;
using Validators;

public class DbContextConfig
{
    /// <summary>
    /// The name of the dbContext
    /// </summary>
    public string ContextName { get; private set; }

    /// <summary>
    /// The name of the database
    /// </summary>
    public string DatabaseName { get; private set; }

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

    public NamingConventionEnum NamingConventionEnum { get; private set; }

    /// <summary>
    /// The naming convention for your database
    /// </summary>
    public string NamingConvention
    {
        get => NamingConventionEnum.Name;
        private set
        {
            if (!NamingConventionEnum.TryFromName(value, true, out var parsed))
            {
                parsed = NamingConventionEnum.SnakeCase;
            }

            NamingConventionEnum = parsed;
        }
    }

    public static DbContextConfig Create(DbContextConfigDto dbContextConfigDto)
    {
        new DbContextConfigDtoValidator().ValidateAndThrow(dbContextConfigDto);
        var mapper = new Mapper(new MapperConfiguration(cfg => { cfg.AddProfile<DbContextConfigProfile>(); }));
        
        var newDbContext = mapper.Map<DbContextConfig>(dbContextConfigDto);
        newDbContext.Provider = dbContextConfigDto.Provider;
        newDbContext.NamingConvention = dbContextConfigDto.NamingConvention;

        return newDbContext;
    }
    
    // public DbContextConfig(DbContextConfigDto dbContextConfigDto)
    // {
    //     new DbContextConfigDtoValidator().ValidateAndThrow(dbContextConfigDto);
    //     var mapper = new Mapper(new MapperConfiguration(cfg => { cfg.AddProfile<DbContextConfigProfile>(); }));
    //
    //     mapper.Map(dbContextConfigDto, this);
    //     Provider = dbContextConfigDto.Provider;
    //     NamingConvention = dbContextConfigDto.NamingConvention;
    // }
}
