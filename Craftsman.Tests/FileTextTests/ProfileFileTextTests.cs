namespace Craftsman.Tests.FileTextTests
{
    using Craftsman.Builders;
    using Craftsman.Tests.Fakes;
    using FluentAssertions;
    using Xunit;

    public class ProfileFileTextTests
    {
        [Fact]
        public void GetProfileFileText_passed_normal_product_entity_creates_expected_text()
        {
            var classNamespace = "Application.Mappings";
            var entity = new FakeEntity().Generate();
            entity.Name = "Product";

            var fileText = EntityMappingBuilder.GetProfileFileText(classNamespace, entity, "", "MyBc");

            var expectedText = @"namespace Application.Mappings
{
    using MyBc.Core.Dtos.Product;
    using MapsterMapper;
    using MyBc.Core.Entities;

    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            //createmap<to this, from this>
            CreateMap<Product, ProductDto>()
                .ReverseMap();
            CreateMap<ProductForCreationDto, Product>();
            CreateMap<ProductForUpdateDto, Product>()
                .ReverseMap();
        }
    }
}";

            fileText.Should().Be(expectedText);
        }
    }
}
