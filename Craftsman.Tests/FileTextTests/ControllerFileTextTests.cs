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

    public class ControllerFileTextTests
    {
        [Fact]
        public void GetControllerFileText_passed_normal_entity_creates_expected_text()
        {
            var classNamespace = "WebApi.Controllers.v1";
            var entity = CannedGenerator.FakeBasicProduct();

            var fileText = ControllerBuilder.GetControllerFileText(classNamespace, entity);

            var expectedText = @$"namespace WebApi.Controllers.v1
{{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using AutoMapper;
    using FluentValidation.AspNetCore;
    using Application.Dtos.Product;
    using Application.Enums;
    using Application.Interfaces.Product;
    using Application.Validation.Product;
    using Domain.Entities;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;

    [ApiController]
    [Route(""api/Products"")]
    [ApiVersion(""1.0"")]
    public class ProductsController: Controller
    {{
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductsController(IProductRepository productRepository
            , IMapper mapper)
        {{
            _productRepository = productRepository ??
                throw new ArgumentNullException(nameof(productRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }}


        [HttpGet(Name = ""GetProducts"")]
        public ActionResult<IEnumerable<ProductDto>> GetProducts([FromQuery] ProductParametersDto productParametersDto)
        {{
            var productsFromRepo = _productRepository.GetProducts(productParametersDto);
            
            var previousPageLink = productsFromRepo.HasPrevious
                    ? CreateProductsResourceUri(productParametersDto,
                        ResourceUriType.PreviousPage)
                    : null;

            var nextPageLink = productsFromRepo.HasNext
                ? CreateProductsResourceUri(productParametersDto,
                    ResourceUriType.NextPage)
                : null;

            var paginationMetadata = new
            {{
                totalCount = productsFromRepo.TotalCount,
                pageSize = productsFromRepo.PageSize,
                pageNumber = productsFromRepo.PageNumber,
                totalPages = productsFromRepo.TotalPages,
                hasPrevious = productsFromRepo.HasPrevious,
                hasNext = productsFromRepo.HasNext,
                previousPageLink,
                nextPageLink
            }};

            Response.Headers.Add(""X-Pagination"",
                JsonSerializer.Serialize(paginationMetadata));

            var productsDto = _mapper.Map<IEnumerable<ProductDto>>(productsFromRepo);
            return Ok(productsDto);
        }}


        [HttpGet(""{{productId}}"", Name = ""GetProduct"")]
        public ActionResult<ProductDto> GetProduct(int productId)
        {{
            var productFromRepo = _productRepository.GetProduct(productId);

            if (productFromRepo == null)
            {{
                return NotFound();
            }}

            var productDto = _mapper.Map<ProductDto>(productFromRepo);

            return Ok(productDto);
        }}

        [HttpPost]
        public ActionResult<ProductDto> AddProduct(ProductForCreationDto productForCreation)
        {{
            var validationResults = new ProductForCreationDtoValidator().Validate(productForCreation);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {{
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }}

            var product = _mapper.Map<Product>(productForCreation);
            _productRepository.AddProduct(product);
            var saveSuccessful = _productRepository.Save();

            if(saveSuccessful)
            {{
                var productDto = _productRepository.GetProduct(product.ProductId); //get from repo for fk object, if needed
                return CreatedAtRoute(""GetProduct"",
                    new {{ productDto.ProductId }},
                    productDto);
            }}

            return StatusCode(500);
        }}

        [HttpDelete(""{{productId}}"")]
        public ActionResult DeleteProduct(int productId)
        {{
            var productFromRepo = _productRepository.GetProduct(productId);

            if (productFromRepo == null)
            {{
                return NotFound();
            }}

            _productRepository.DeleteProduct(productFromRepo);
            _productRepository.Save();

            return NoContent();
        }}

        [HttpPut(""{{productId}}"")]
        public IActionResult UpdateProduct(int productId, ProductForUpdateDto product)
        {{
            var productFromRepo = _productRepository.GetProduct(productId);

            if (productFromRepo == null)
            {{
                return NotFound();
            }}

            var validationResults = new ProductForUpdateDtoValidator().Validate(product);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {{
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }}

            _mapper.Map(product, productFromRepo);
            _productRepository.UpdateProduct(productFromRepo);

            _productRepository.Save();

            return NoContent();
        }}

        [HttpPatch(""{{productId}}"")]
        public IActionResult PartiallyUpdateProduct(int productId, JsonPatchDocument<ProductForUpdateDto> patchDoc)
        {{
            if (patchDoc == null)
            {{
                return BadRequest();
            }}

            var existingProduct = _productRepository.GetProduct(productId);

            if (existingProduct == null)
            {{
                return NotFound();
            }}

            var productToPatch = _mapper.Map<ProductForUpdateDto>(existingProduct); // map the product we got from the database to an updatable product model
            patchDoc.ApplyTo(productToPatch, ModelState); // apply patchdoc updates to the updatable product

            if (!TryValidateModel(productToPatch))
            {{
                return ValidationProblem(ModelState);
            }}

            _mapper.Map(productToPatch, existingProduct); // apply updates from the updatable product to the db entity so we can apply the updates to the database
            _productRepository.UpdateProduct(existingProduct); // apply business updates to data if needed

            _productRepository.Save(); // save changes in the database

            return NoContent();
        }}

        private string CreateProductsResourceUri(
            ProductParametersDto productParametersDto,
            ResourceUriType type)
        {{
            switch (type)
            {{
                case ResourceUriType.PreviousPage:
                    return Url.Link(""GetProducts"",
                        new
                        {{
                            filters = productParametersDto.Filters,
                            orderBy = productParametersDto.SortOrder,
                            pageNumber = productParametersDto.PageNumber - 1,
                            pageSize = productParametersDto.PageSize
                        }});
                case ResourceUriType.NextPage:
                    return Url.Link(""GetProducts"",
                        new
                        {{
                            filters = productParametersDto.Filters,
                            orderBy = productParametersDto.SortOrder,
                            pageNumber = productParametersDto.PageNumber + 1,
                            pageSize = productParametersDto.PageSize
                        }});

                default:
                    return Url.Link(""GetProducts"",
                        new
                        {{
                            filters = productParametersDto.Filters,
                            orderBy = productParametersDto.SortOrder,
                            pageNumber = productParametersDto.PageNumber,
                            pageSize = productParametersDto.PageSize
                        }});
            }}
        }}
    }}
}}";

            fileText.Should().Be(expectedText);
        }
    }
}
