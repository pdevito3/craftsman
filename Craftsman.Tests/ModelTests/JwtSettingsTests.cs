namespace Craftsman.Tests.ModelTests
{
    using Craftsman.Models;
    using Craftsman.Tests.Fakes;
    using FluentAssertions;
    using NuGet.Frameworks;
    using System.Collections.Generic;
    using Xunit;

    public class JwtSettingsTests
    {
        [Fact]
        public void JwtSettings_sets_default_values()
        {
            var settings = new JwtSettings() {};

            settings.Key.Should().NotBeNullOrEmpty();
            settings.Audience.Should().NotBeNullOrEmpty();
            settings.Issuer.Should().NotBeNullOrEmpty();
            settings.DurationInMinutes.Should().Be(60);
        }

        [Fact]
        public void JwtSettings_sets_key_when_given()
        {
            var key = "my key";
            var settings = new JwtSettings() { };
            settings.Key = key;

            settings.Key.Should().Be(key);
        }
    }
}
