namespace NewCraftsmanUnitTests;

using Fakes;
using FluentAssertions;
using NewCraftsman.Domain.BoundedContexts;
using NUnit.Framework;

public class BoundedContextTests
{
    [Test]
    public void project_name_required()
    {
        var dto = new FakeBoundedContextDto().Generate();
        dto.ProjectName = null;

        FluentActions
            .Invoking( () =>
            {
                BoundedContext.Create(dto);
            })
            .Should()
            .Throw<FluentValidation.ValidationException>();
    }
}