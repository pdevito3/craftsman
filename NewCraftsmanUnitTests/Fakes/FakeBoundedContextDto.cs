namespace NewCraftsmanUnitTests.Fakes;

using AutoBogus;
using NewCraftsman.Domain.BoundedContexts.Dtos;

public class FakeBoundedContextDto : AutoFaker<BoundedContextDto>
{
    public FakeBoundedContextDto()
    {
        // if you want default values on any of your properties (e.g. an int between a certain range or a date always in the past), you can add `RuleFor` lines describing those defaults
        //RuleFor(c => c.ExampleIntProperty, c => c.Random.Number(50, 100000));
        //RuleFor(c => c.ExampleDateProperty, c => c.Date.Past());
    }
}