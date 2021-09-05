namespace Craftsman.Tests.ModelTests
{
    using Craftsman.Models;
    using FluentAssertions;
    using Xunit;

    public class EntityPropertyTests
    {
        [Fact]
        public void property_is_required_when_marked_true_and_not_pk()
        {
            var prop = new EntityProperty()
            {
                IsRequired = true
            };

            prop.IsRequired.Should().Be(true);
        }

        [Theory]
        [InlineData("keyname",true)]
        [InlineData(null, false)]
        public void ISForiegnKeyAssignedAppropriately(string keyName, bool isFk)
        {
            var prop = new EntityProperty()
            {
                ForeignEntityName = keyName
            };

            prop.IsForeignKey.Should().Be(isFk);
        }
    }
}
