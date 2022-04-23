namespace NewCraftsmanUnitTests;

using Craftsman.Domain;
using Fakes;
using FluentAssertions;
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
                ApiTemplate.Create(dto);
            })
            .Should()
            .Throw<FluentValidation.ValidationException>();
    }
}