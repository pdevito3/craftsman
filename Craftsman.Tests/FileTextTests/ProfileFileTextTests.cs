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

    public class ProfileFileTextTests
    {
        [Fact]
        public void GetProfileFileText_passed_normal_product_entity_creates_expected_text()
        {
            var classNamespace = "Application.Mappings";
            var entity = new FakeEntity().Generate();
            entity.Name = "Product";

            var fileText = ProfileBuilder.GetProfileFileText(classNamespace, entity);

            var expectedText = @"namespace Application.Mappings
{
    using Application.Dtos.Product;
    using AutoMapper;
    using Domain.Entities;

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
