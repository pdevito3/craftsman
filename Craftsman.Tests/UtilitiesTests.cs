namespace Craftsman.Tests
{
    using System;
    using FluentAssertions;
    using Helpers;
    using Models;
    using Xunit;

    public class UtilitiesTests
    {
        [Theory]
        [InlineData("guid", "", " = Guid.NewGuid();", "")]
        [InlineData("guid", "", "", "something")]
        [InlineData("guid?", "", "", "")]
        [InlineData("guid", "something", @" = Guid.Parse(""something"");", "")]
        public void EntityBuilder_CreateEntity_createsEntityFile(string propType, string defaultValue, string expectedDefaultValue, string fkEntityName)
        {
            var prop = new EntityProperty() {Type = propType, ForeignEntityName = fkEntityName};
            
            var actualPropValue = Utilities.GetDefaultValueText(defaultValue, prop);
            actualPropValue.Should().Be(expectedDefaultValue);
        }
    }
}
