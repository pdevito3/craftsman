namespace NewCraftsman.Domain.DomainProjects.Mappings;

using AutoMapper;
using Dtos;

public class DomainProjectProfile : Profile
{
    public DomainProjectProfile()
    {
        //createmap<to this, from this>
        CreateMap<DomainProject, DomainProjectDto>()
            .ReverseMap();
    }
}