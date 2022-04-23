namespace Craftsman.Tests.ModelTests
{
    using Craftsman.Exceptions;
    using Domain;
    using FluentAssertions;
    using System;
    using Bogus;
    using Xunit;

    public class TemplateDbContextTests
    {
        [Fact]
        public void DbProviderAssignedAccurately()
        {
            var lowerConfig = new DbContextConfig()
            {
                Provider = "sqlserver"
            };
            var mysqlConfig = new DbContextConfig()
            {
                Provider = "mysql"
            };
            var postgresConfig = new DbContextConfig()
            {
                Provider = "postgres"
            };
            var partialCasingConfig = new DbContextConfig()
            {
                Provider = "Sqlserver"
            };
            var properCasingConfig = new DbContextConfig()
            {
                Provider = "SqlServer"
            };

            lowerConfig.ProviderEnum.Should().Be(DbProvider.SqlServer);
            mysqlConfig.ProviderEnum.Should().Be(DbProvider.MySql);
            postgresConfig.ProviderEnum.Should().Be(DbProvider.Postgres);
            partialCasingConfig.ProviderEnum.Should().Be(DbProvider.SqlServer);
            properCasingConfig.ProviderEnum.Should().Be(DbProvider.SqlServer);
        }

        [Fact]
        public void DbProviderThrowsExceptionWhenInvalid()
        {
            Action actEmpty = () => new DbContextConfig()
            {
                Provider = ""
            };
            Action actBad = () => new DbContextConfig()
            {
                Provider = new Faker().Lorem.Word()
            };

            actEmpty.Should().Throw<InvalidDbProviderException>();
            actBad.Should().Throw<InvalidDbProviderException>();
        }
    }
}
