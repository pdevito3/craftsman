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

    public class EntityFileTextTests
    {
        [Fact]
        public void GetEntityFileText_passed_normal_entity_creates_expected_text()
        {
            var classNamespace = "Domain.Entities";
            var entity = CannedGenerator.FakeBasicProduct();

            var fileText = EntityBuilder.GetEntityFileText(classNamespace, entity);

            var expectedText = @"namespace Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Sieve.Attributes;

    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [Sieve(CanFilter = true, CanSort = false)]
        public int ProductId { get; set; }

        [Sieve(CanFilter = true, CanSort = false)]
        public string Name { get; set; }

        // add-on property marker - Do Not Delete This Comment
    }
}";

            fileText.Should().Be(expectedText);
        }
    }
}
