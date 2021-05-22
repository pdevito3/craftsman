namespace Craftsman.Tests.ModelTests
{
    using Craftsman.Models;
    using Craftsman.Tests.Fakes;
    using FluentAssertions;
    using Xunit;

    public class EntityTests
    {
        [Fact]
        public void PluralPropertyDefaultsToEntityNameWithAnS()
        {
            var name = "Name";
            var entity = new Entity()
            {
                Name = name
            };

            entity.Plural.Should().Be($"{name}s");
        }

        [Fact]
        public void PluralPropertySetToGivenValueAndNotDefault()
        {
            var plural = "Cities";
            var entity = new Entity()
            {
                Plural = plural
            };

            entity.Plural.Should().Be(plural);
        }

        [Fact]
        public void LambdaIsLowerFirstLetterIfNotGiven()
        {
            var name = "Name";
            var entity = new Entity()
            {
                Name = name
            };

            entity.Lambda.Should().BeEquivalentTo("n");
        }

        [Fact]
        public void PrimaryKeyAutoAssigned()
        {
            var entity = new FakeEntity { }.Generate();
            var prop = new FakeEntityProperty { }.Generate();
            prop.IsPrimaryKey = true;
            entity.Properties.Clear();
            entity.Properties.Add(prop);

            entity.PrimaryKeyProperty.Should().BeEquivalentTo(prop);
        }
    }
}
