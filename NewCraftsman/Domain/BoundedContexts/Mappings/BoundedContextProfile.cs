namespace NewCraftsman.Domain.BoundedContexts.Mappings;

using AutoMapper;
using Dtos;

public class BoundedContextProfile : Profile
{
    public BoundedContextProfile()
    {
        //createmap<to this, from this>
        CreateMap<BoundedContext, BoundedContextDto>()
            .ReverseMap();
    }
}