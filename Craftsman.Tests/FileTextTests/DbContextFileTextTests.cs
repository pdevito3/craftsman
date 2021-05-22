namespace Craftsman.Tests.FileTextTests
{
    using Craftsman.Builders;
    using Craftsman.Tests.Fakes;
    using FluentAssertions;
    using Xunit;

    public class DbContextFileTextTests
    {
        [Fact]
        public void GetContextFileText_passed_normal_entity_creates_expected_text()
        {
            var classNamespace = "Infrastructure.Persistence.Contexts";
            var template = CannedGenerator.FakeBasicApiTemplate();

            var fileText = DbContextBuilder.GetContextFileText(classNamespace, template.Entities, template.DbContext.ContextName, "", "MyBc");

            var expectedText = @$"namespace Infrastructure.Persistence.Contexts
{{
    using MyBc.Core.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using System.Threading;
    using System.Threading.Tasks;

    public class BespokedBikesDbContext : DbContext
    {{
        public BespokedBikesDbContext(
            DbContextOptions<BespokedBikesDbContext> options) : base(options) 
        {{
        }}

        #region DbSet Region - Do Not Delete
        public DbSet<Product> Products {{ get; set; }}
        #endregion        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {{
            modelBuilder.Entity<Product>().Property(p => p.ProductId).ValueGeneratedNever();
        }}
    }}
}}";

            fileText.Should().Be(expectedText);
        }
    }
}
