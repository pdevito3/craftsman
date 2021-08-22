namespace Craftsman.Tests
{
    using System;
    using FluentAssertions;
    using Helpers;
    using Xunit;

    public class UtilitiesTests
    {
        [Theory]
        [InlineData("guid", "", " = Guid.NewGuid();")]
        [InlineData("guid?", "", "")]
        [InlineData("guid", "something", @" = Guid.Parse(""something"");")]
        public void EntityBuilder_CreateEntity_createsEntityFile(string propType, string defaultValue, string expectedDefaultValue)
        {
            var actualPropValue = Utilities.GetDefaultValueText(defaultValue, propType);
            actualPropValue.Should().Be(expectedDefaultValue);
        }
    }
}
