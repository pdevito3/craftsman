namespace NewCraftsman.Domain.DbContextConfigs.Mappings;

using AutoMapper;
using Dtos;

public class DbContextConfigProfile : Profile
{
    public DbContextConfigProfile()
    {
        //createmap<to this, from this>
        CreateMap<DbContextConfig, DbContextConfigDto>()
            .ReverseMap();
    }
}