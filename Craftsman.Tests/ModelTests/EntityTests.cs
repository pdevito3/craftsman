namespace Craftsman.Tests.ModelTests
{
    using Domain;
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
    }
}
