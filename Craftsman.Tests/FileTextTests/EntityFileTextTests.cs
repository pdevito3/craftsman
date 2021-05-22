namespace Craftsman.Tests.FileTextTests
{
    using Craftsman.Builders;
    using Craftsman.Models;
    using Craftsman.Tests.Fakes;
    using FluentAssertions;
    using Xunit;

    public class EntityFileTextTests
    {
        [Fact]
        public void GetEntityFileText_passed_normal_entity_creates_expected_text()
        {
            var classNamespace = "Domain.Entities";
            var entity = CannedGenerator.FakeBasicProduct();
            var tableAnnotation = EntityBuilder.TableAnnotationBuilder(entity);
            var fileText = EntityBuilder.GetEntityFileText(classNamespace, entity);

            var expectedText = @$"namespace Domain.Entities
{{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Sieve.Attributes;

    {tableAnnotation}
    public class Product
    {{
        [Key]
        [Required]
        [Sieve(CanFilter = true, CanSort = false)]
        public int ProductId {{ get; set; }}

        [Sieve(CanFilter = true, CanSort = false)]
        public string Name {{ get; set; }}

        // add-on property marker - Do Not Delete This Comment
    }}
}}";

            fileText.Should().Be(expectedText);
        }

        [Theory]
        [InlineData("bool", "false", "public bool Test { get; set; } = false;")]
        [InlineData("string", @"""test""", @"public string Test { get; set; } = ""test"";")]
        public void GetEntityFileText_passed_entity_with_default_value_creates_expected_text(string type, string defaultVal, string expectedPropertyText)
        {
            var classNamespace = "Domain.Entities";
            var entity = CannedGenerator.FakeBasicProduct();
            entity.Properties.Add(new EntityProperty { Name = "Test", Type = type, DefaultValue = defaultVal, CanFilter = true, CanSort = true });

            var tableAnnotation = EntityBuilder.TableAnnotationBuilder(entity);
            var fileText = EntityBuilder.GetEntityFileText(classNamespace, entity);

            var expectedText = @$"namespace Domain.Entities
{{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Sieve.Attributes;

    {tableAnnotation}
    public class Product
    {{
        [Key]
        [Required]
        [Sieve(CanFilter = true, CanSort = false)]
        public int ProductId {{ get; set; }}

        [Sieve(CanFilter = true, CanSort = false)]
        public string Name {{ get; set; }}

        [Sieve(CanFilter = true, CanSort = true)]
        {expectedPropertyText}

        // add-on property marker - Do Not Delete This Comment
    }}
}}";

            fileText.Should().Be(expectedText);
        }

        [Fact]
        public void GetEntityFileText_passed_custom_property_column_name_entity_creates_expected_text()
        {
            var classNamespace = "Domain.Entities";
            var entity = CannedGenerator.FakeBasicProduct();
            entity.Properties.Add(new EntityProperty { Name = "ProductType", Type = "int", CanFilter = true, CanSort = true, ColumnName = "Product_Type" });

            var fileText = EntityBuilder.GetEntityFileText(classNamespace, entity);

            var expectedText = @$"namespace Domain.Entities
{{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Sieve.Attributes;

    [Table(""Product"")]
    public class Product
    {{
        [Key]
        [Required]
        [Sieve(CanFilter = true, CanSort = false)]
        public int ProductId {{ get; set; }}

        [Sieve(CanFilter = true, CanSort = false)]
        public string Name {{ get; set; }}

        [Sieve(CanFilter = true, CanSort = true)]
        [Column(""Product_Type"")]
        public int ProductType {{ get; set; }}

        // add-on property marker - Do Not Delete This Comment
    }}
}}";

            fileText.Should().Be(expectedText);
        }
    }
}
