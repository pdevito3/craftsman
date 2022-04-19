namespace NewCraftsmanUnitTests.Fakes;

using AutoBogus;
using NewCraftsman.Domain;
using NewCraftsman.Domain.DbContextConfigs.Dtos;

public class FakeDbContextConfigDto : AutoFaker<DbContextConfigDto>
{
    public FakeDbContextConfigDto()
    {
        RuleFor(e => e.Provider, 
            u => u.PickRandom<DbProvider>(DbProvider.List).Name);
        RuleFor(e => e.NamingConvention, 
            u => u.PickRandom<NamingConventionEnum>(NamingConventionEnum.List).Name);
    }
}