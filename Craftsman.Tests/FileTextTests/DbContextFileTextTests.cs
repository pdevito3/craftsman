namespace Craftsman.Tests.FileTextTests
{
    using Craftsman.Builders;
    using Craftsman.Models;
    using Craftsman.Tests.Fakes;
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using Xunit;
    using System.Linq;
    using AutoBogus;

    public class DbContextFileTextTests
    {
        [Fact]
        public void GetContextFileText_passed_normal_entity_creates_expected_text()
        {
            var classNamespace = "Infrastructure.Persistence.Contexts";
            var template = CannedGenerator.FakeBasicApiTemplate();

            var fileText = DbContextBuilder.GetContextFileText(classNamespace, template.Entities, template.DbContext.ContextName, template.AuthSetup.AuthMethod);

            var expectedText = @"namespace Infrastructure.Persistence.Contexts
{
    using Application.Interfaces;
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using System.Threading;
    using System.Threading.Tasks;

    public class BespokedBikesDbContext : DbContext
    {
        public BespokedBikesDbContext(
            DbContextOptions<BespokedBikesDbContext> options) : base(options) 
        {
        }

        #region DbSet Region - Do Not Delete
        public DbSet<Product> Products { get; set; }
        #endregion
    }
}";

            fileText.Should().Be(expectedText);
        }
    }
}
