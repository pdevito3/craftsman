namespace Craftsman.Tests.ModelTests
{
    using Craftsman.Exceptions;
    using Domain;
    using FluentAssertions;
    using System;
    using Xunit;

    public class TemplateDbContextTests
    {
        [Theory]
        [InlineData("sqlserver", DbProvider.SqlServer)]
        [InlineData("mysql", DbProvider.MySql)]
        [InlineData("postgres", DbProvider.Postgres)]
        [InlineData("Sqlserver", DbProvider.SqlServer)]
        [InlineData("SqlServer", DbProvider.SqlServer)]
        public void DbProviderAssignedAccurately(string providerString, DbProvider expectedProvider)
        {
            var context = new DbContextConfig()
            {
                Provider = providerString
            };

            var expectedProviderString = Enum.GetName(typeof(DbProvider), expectedProvider);
            context.Provider.Should().Be(expectedProviderString);
        }

        [Theory]
        [InlineData("", DbProvider.SqlServer)]
        [InlineData("hjgujafha", DbProvider.SqlServer)]
        public void DbProviderThrowsExceptionWhenInvalid(string providerString, DbProvider expectedProvider)
        {
            Action act = () => new DbContextConfig()
            {
                Provider = providerString
            };

            var expectedProviderString = Enum.GetName(typeof(DbProvider), expectedProvider);
            act.Should().Throw<InvalidDbProviderException>();
        }
    }
}
