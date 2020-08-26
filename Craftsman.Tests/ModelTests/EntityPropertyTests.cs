namespace Craftsman.Tests.ModelTests
{
    using Craftsman.Models;
    using FluentAssertions;
    using Xunit;

    public class EntityPropertyTests
    {
        [Fact]
        public void PropertyIsRequiredAndCanNotBeManipulatedIfIsPrimaryKey()
        {
            var prop = new EntityProperty()
            {
                IsPrimaryKey = true
            };

            prop.IsRequired.Should().Be(true);
            prop.CanManipulate.Should().Be(false);
        }

        [Theory]
        [InlineData("keyname",true)]
        [InlineData(null, false)]
        public void ISForiegnKeyAssignedAppropriately(string keyname, bool isFk)
        {
            var prop = new EntityProperty()
            {
                ForeignKeyPropName = keyname
            };

            prop.IsForeignKey.Should().Be(isFk);
        }
    }
}
