namespace Craftsman.Tests.ModelTests
{
    using Craftsman.Models;
    using FluentAssertions;
    using Xunit;

    public class ApiEnvironmentTests
    {
        [Theory]
        [InlineData("startup","Startup")]
        [InlineData("Startup", "Startup")]
        [InlineData("QA", "QA")]
        [InlineData("Dev", "Dev")]
        [InlineData("Development", "Development")]
        public void EnvironmentName_generated_accurately(string val, string expected)
        {
            var prop = new ApiEnvironment()
            {
                EnvironmentName = val
            };

            prop.EnvironmentName.Should().Be(expected);
        }
    }
}
