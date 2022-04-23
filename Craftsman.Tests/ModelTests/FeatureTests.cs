namespace Craftsman.Tests.ModelTests
{
    using Domain;
    using Craftsman.Tests.Fakes;
    using Exceptions;
    using FluentAssertions;
    using NSubstitute.ExceptionExtensions;
    using Xunit;

    public class FeatureTests
    {
        [Fact]
        public void create_record_type_does_not_throw_invalid_error()
        {
            var feature = new Feature()
            {
                Type = "CreateRecord"
            };

            true.Should().BeTrue();// if hit then no error was thrown
        }
        
        [Fact]
        public void invalid_feature_type_throws_error()
        {
            FluentActions.Invoking(() =>
            {
                var feature = new Feature()
                {
                    Type = "invalid"
                };
            }).Should().Throw<InvalidFeatureTypeException>();
        }
    }
}
