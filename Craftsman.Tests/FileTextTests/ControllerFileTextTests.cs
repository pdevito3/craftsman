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
        public void GetControllerFileText_passed_normal_entity_creates_expected_text_without_swagger()
        {
            var classNamespace = "WebApi.Controllers.v1";
            var entity = CannedGenerator.FakeBasicProduct();

            var fileText = ControllerBuilder.GetControllerFileText(classNamespace, entity, false, new List<Policy>(), "", "");

            var expectedText = @$"namespace WebApi.Controllers.v1
{{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;
    using .Core.Dtos.Product;
    using .Core.Wrappers;
    using System.Threading;
    using MediatR;
    using static .WebApi.Features.Products.GetProductList;
    using static .WebApi.Features.Products.GetProduct;
    using static .WebApi.Features.Products.AddProduct;
    using static .WebApi.Features.Products.DeleteProduct;
    using static .WebApi.Features.Products.UpdateProduct;
    using static .WebApi.Features.Products.PatchProduct;

    [ApiController]
    [Route(""api/Products"")]
    [ApiVersion(""1.0"")]
    public class ProductsController: Controller
    {{
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {{
            _mediator = mediator;
        }}
        
        [Consumes(""application/json"")]
        [Produces(""application/json"")]
        [HttpGet(Name = ""GetProducts"")]
        public async Task<IActionResult> GetProducts([FromQuery] ProductParametersDto productParametersDto)
        {{
            // add error handling
            var query = new ProductListQuery(productParametersDto);
            var queryResponse = await _mediator.Send(query);

            var paginationMetadata = new
            {{
                totalCount = queryResponse.TotalCount,
                pageSize = queryResponse.PageSize,
                currentPageSize = queryResponse.CurrentPageSize,
                currentStartIndex = queryResponse.CurrentStartIndex,
                currentEndIndex = queryResponse.CurrentEndIndex,
                pageNumber = queryResponse.PageNumber,
                totalPages = queryResponse.TotalPages,
                hasPrevious = queryResponse.HasPrevious,
                hasNext = queryResponse.HasNext
            }};

            Response.Headers.Add(""X-Pagination"",
                JsonSerializer.Serialize(paginationMetadata));

            var response = new Response<IEnumerable<ProductDto>>(queryResponse);
            return Ok(response);
        }}
        
        [Produces(""application/json"")]
        [HttpGet(""{{productId}}"", Name = ""GetProduct"")]
        public async Task<ActionResult<ProductDto>> GetProduct(int productId)
        {{
            // add error handling
            var query = new ProductQuery(productId);
            var queryResponse = await _mediator.Send(query);

            var response = new Response<ProductDto>(queryResponse);
            return Ok(response);
        }}
        
        [Consumes(""application/json"")]
        [Produces(""application/json"")]
        [HttpPost]
        public async Task<ActionResult<ProductDto>> AddProduct([FromBody]ProductForCreationDto productForCreation)
        {{
            // add error handling
            var command = new AddProductCommand(productForCreation);
            var commandResponse = await _mediator.Send(command);
            var response = new Response<ProductDto>(commandResponse);

            return CreatedAtRoute(""GetProduct"",
                new {{ commandResponse.ProductId }},
                response);
        }}
        
        [Produces(""application/json"")]
        [HttpDelete(""{{productId}}"")]
        public async Task<ActionResult> DeleteProduct(int productId)
        {{
            // add error handling
            var command = new DeleteProductCommand(productId);
            await _mediator.Send(command);

            return NoContent();
        }}
        
        [Produces(""application/json"")]
        [HttpPut(""{{productId}}"")]
        public async Task<IActionResult> UpdateProduct(int productId, ProductForUpdateDto product)
        {{
            // add error handling
            var command = new UpdateProductCommand(productId, product);
            await _mediator.Send(command);

            return NoContent();
        }}
        
        [Consumes(""application/json"")]
        [Produces(""application/json"")]
        [HttpPatch(""{{productId}}"")]
        public async Task<IActionResult> PartiallyUpdateProduct(int productId, JsonPatchDocument<ProductForUpdateDto> patchDoc)
        {{
            // add error handling
            var command = new PatchProductCommand(productId, patchDoc);
            await _mediator.Send(command);

            return NoContent();
        }}
    }}
}}";

            fileText.Should().Be(expectedText);
        }


        [Fact]
        public void GetControllerFileText_passed_entity_with_same_name_as_plural_creates_expected_text()
        {
            var classNamespace = "WebApi.Controllers.v1";
            var entity = CannedGenerator.FakeBasicProduct();
            entity.Name = "Buffalo";
            entity.Plural = "Buffalo";

            var fileText = ControllerBuilder.GetControllerFileText(classNamespace, entity, false, new List<Policy>(), "", "");

            var expectedText = @$"namespace WebApi.Controllers.v1
{{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;
    using .Core.Dtos.Buffalo;
    using .Core.Wrappers;
    using System.Threading;
    using MediatR;
    using static .WebApi.Features.Buffalo.GetBuffaloList;
    using static .WebApi.Features.Buffalo.GetBuffalo;
    using static .WebApi.Features.Buffalo.AddBuffalo;
    using static .WebApi.Features.Buffalo.DeleteBuffalo;
    using static .WebApi.Features.Buffalo.UpdateBuffalo;
    using static .WebApi.Features.Buffalo.PatchBuffalo;

    [ApiController]
    [Route(""api/Buffalo"")]
    [ApiVersion(""1.0"")]
    public class BuffaloController: Controller
    {{
        private readonly IMediator _mediator;

        public BuffaloController(IMediator mediator)
        {{
            _mediator = mediator;
        }}
        
        [Consumes(""application/json"")]
        [Produces(""application/json"")]
        [HttpGet(Name = ""GetBuffaloList"")]
        public async Task<IActionResult> GetBuffalo([FromQuery] BuffaloParametersDto buffaloParametersDto)
        {{
            // add error handling
            var query = new BuffaloListQuery(buffaloParametersDto);
            var queryResponse = await _mediator.Send(query);

            var paginationMetadata = new
            {{
                totalCount = queryResponse.TotalCount,
                pageSize = queryResponse.PageSize,
                currentPageSize = queryResponse.CurrentPageSize,
                currentStartIndex = queryResponse.CurrentStartIndex,
                currentEndIndex = queryResponse.CurrentEndIndex,
                pageNumber = queryResponse.PageNumber,
                totalPages = queryResponse.TotalPages,
                hasPrevious = queryResponse.HasPrevious,
                hasNext = queryResponse.HasNext
            }};

            Response.Headers.Add(""X-Pagination"",
                JsonSerializer.Serialize(paginationMetadata));

            var response = new Response<IEnumerable<BuffaloDto>>(queryResponse);
            return Ok(response);
        }}
        
        [Produces(""application/json"")]
        [HttpGet(""{{buffaloId}}"", Name = ""GetBuffaloRecord"")]
        public async Task<ActionResult<BuffaloDto>> GetBuffalo(int buffaloId)
        {{
            // add error handling
            var query = new BuffaloQuery(buffaloId);
            var queryResponse = await _mediator.Send(query);

            var response = new Response<BuffaloDto>(queryResponse);
            return Ok(response);
        }}
        
        [Consumes(""application/json"")]
        [Produces(""application/json"")]
        [HttpPost]
        public async Task<ActionResult<BuffaloDto>> AddBuffalo([FromBody]BuffaloForCreationDto buffaloForCreation)
        {{
            // add error handling
            var command = new AddBuffaloCommand(buffaloForCreation);
            var commandResponse = await _mediator.Send(command);
            var response = new Response<BuffaloDto>(commandResponse);

            return CreatedAtRoute(""GetBuffalo"",
                new {{ commandResponse.ProductId }},
                response);
        }}
        
        [Produces(""application/json"")]
        [HttpDelete(""{{buffaloId}}"")]
        public async Task<ActionResult> DeleteBuffalo(int buffaloId)
        {{
            // add error handling
            var command = new DeleteBuffaloCommand(buffaloId);
            await _mediator.Send(command);

            return NoContent();
        }}
        
        [Produces(""application/json"")]
        [HttpPut(""{{buffaloId}}"")]
        public async Task<IActionResult> UpdateBuffalo(int buffaloId, BuffaloForUpdateDto buffalo)
        {{
            // add error handling
            var command = new UpdateBuffaloCommand(buffaloId, buffalo);
            await _mediator.Send(command);

            return NoContent();
        }}
        
        [Consumes(""application/json"")]
        [Produces(""application/json"")]
        [HttpPatch(""{{buffaloId}}"")]
        public async Task<IActionResult> PartiallyUpdateBuffalo(int buffaloId, JsonPatchDocument<BuffaloForUpdateDto> patchDoc)
        {{
            // add error handling
            var command = new PatchBuffaloCommand(buffaloId, patchDoc);
            await _mediator.Send(command);

            return NoContent();
        }}
    }}
}}";

            fileText.Should().Be(expectedText);
        }

        [Fact]
        public void GetControllerFileText_passed_normal_entity_creates_expected_text_with_swagger_comments()
        {
            var classNamespace = "WebApi.Controllers.v1";
            var entity = CannedGenerator.FakeBasicProduct();

            var fileText = ControllerBuilder.GetControllerFileText(classNamespace, entity, true, new List<Policy>(), "", "");

            var expectedText = @$"namespace WebApi.Controllers.v1
{{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;
    using .Core.Dtos.Product;
    using .Core.Wrappers;
    using System.Threading;
    using MediatR;
    using static .WebApi.Features.Products.GetProductList;
    using static .WebApi.Features.Products.GetProduct;
    using static .WebApi.Features.Products.AddProduct;
    using static .WebApi.Features.Products.DeleteProduct;
    using static .WebApi.Features.Products.UpdateProduct;
    using static .WebApi.Features.Products.PatchProduct;

    [ApiController]
    [Route(""api/Products"")]
    [ApiVersion(""1.0"")]
    public class ProductsController: Controller
    {{
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {{
            _mediator = mediator;
        }}
        
        /// <summary>
        /// Gets a list of all Products.
        /// </summary>
        /// <response code=""200"">Product list returned successfully.</response>
        /// <response code=""400"">Product has missing/invalid values.</response>
        /// <response code=""500"">There was an error on the server while creating the Product.</response>
        /// <remarks>
        /// Requests can be narrowed down with a variety of query string values:
        /// ## Query String Parameters
        /// - **PageNumber**: An integer value that designates the page of records that should be returned.
        /// - **PageSize**: An integer value that designates the number of records returned on the given page that you would like to return. This value is capped by the internal MaxPageSize.
        /// - **SortOrder**: A comma delimited ordered list of property names to sort by. Adding a `-` before the name switches to sorting descendingly.
        /// - **Filters**: A comma delimited list of fields to filter by formatted as `{{Name}}{{Operator}}{{Value}}` where
        ///     - {{Name}} is the name of a filterable property. You can also have multiple names (for OR logic) by enclosing them in brackets and using a pipe delimiter, eg. `(LikeCount|CommentCount)>10` asks if LikeCount or CommentCount is >10
        ///     - {{Operator}} is one of the Operators below
        ///     - {{Value}} is the value to use for filtering. You can also have multiple values (for OR logic) by using a pipe delimiter, eg.`Title@= new|hot` will return posts with titles that contain the text ""new"" or ""hot""
        ///
        ///    | Operator | Meaning                       | Operator  | Meaning                                      |
        ///    | -------- | ----------------------------- | --------- | -------------------------------------------- |
        ///    | `==`     | Equals                        |  `!@=`    | Does not Contains                            |
        ///    | `!=`     | Not equals                    |  `!_=`    | Does not Starts with                         |
        ///    | `>`      | Greater than                  |  `@=*`    | Case-insensitive string Contains             |
        ///    | `&lt;`   | Less than                     |  `_=*`    | Case-insensitive string Starts with          |
        ///    | `>=`     | Greater than or equal to      |  `==*`    | Case-insensitive string Equals               |
        ///    | `&lt;=`  | Less than or equal to         |  `!=*`    | Case-insensitive string Not equals           |
        ///    | `@=`     | Contains                      |  `!@=*`   | Case-insensitive string does not Contains    |
        ///    | `_=`     | Starts with                   |  `!_=*`   | Case-insensitive string does not Starts with |
        /// </remarks>
        [ProducesResponseType(typeof(Response<IEnumerable<ProductDto>>), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Consumes(""application/json"")]
        [Produces(""application/json"")]
        [HttpGet(Name = ""GetProducts"")]
        public async Task<IActionResult> GetProducts([FromQuery] ProductParametersDto productParametersDto)
        {{
            // add error handling
            var query = new ProductListQuery(productParametersDto);
            var queryResponse = await _mediator.Send(query);

            var paginationMetadata = new
            {{
                totalCount = queryResponse.TotalCount,
                pageSize = queryResponse.PageSize,
                currentPageSize = queryResponse.CurrentPageSize,
                currentStartIndex = queryResponse.CurrentStartIndex,
                currentEndIndex = queryResponse.CurrentEndIndex,
                pageNumber = queryResponse.PageNumber,
                totalPages = queryResponse.TotalPages,
                hasPrevious = queryResponse.HasPrevious,
                hasNext = queryResponse.HasNext
            }};

            Response.Headers.Add(""X-Pagination"",
                JsonSerializer.Serialize(paginationMetadata));

            var response = new Response<IEnumerable<ProductDto>>(queryResponse);
            return Ok(response);
        }}
        
        /// <summary>
        /// Gets a single Product by ID.
        /// </summary>
        /// <response code=""200"">Product record returned successfully.</response>
        /// <response code=""400"">Product has missing/invalid values.</response>
        /// <response code=""500"">There was an error on the server while creating the Product.</response>
        [ProducesResponseType(typeof(Response<ProductDto>), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Produces(""application/json"")]
        [HttpGet(""{{productId}}"", Name = ""GetProduct"")]
        public async Task<ActionResult<ProductDto>> GetProduct(int productId)
        {{
            // add error handling
            var query = new ProductQuery(productId);
            var queryResponse = await _mediator.Send(query);

            var response = new Response<ProductDto>(queryResponse);
            return Ok(response);
        }}
        
        /// <summary>
        /// Creates a new Product record.
        /// </summary>
        /// <response code=""201"">Product created.</response>
        /// <response code=""400"">Product has missing/invalid values.</response>
        /// <response code=""500"">There was an error on the server while creating the Product.</response>
        [ProducesResponseType(typeof(Response<ProductDto>), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Consumes(""application/json"")]
        [Produces(""application/json"")]
        [HttpPost]
        public async Task<ActionResult<ProductDto>> AddProduct([FromBody]ProductForCreationDto productForCreation)
        {{
            // add error handling
            var command = new AddProductCommand(productForCreation);
            var commandResponse = await _mediator.Send(command);
            var response = new Response<ProductDto>(commandResponse);

            return CreatedAtRoute(""GetProduct"",
                new {{ commandResponse.ProductId }},
                response);
        }}
        
        /// <summary>
        /// Deletes an existing Product record.
        /// </summary>
        /// <response code=""204"">Product deleted.</response>
        /// <response code=""400"">Product has missing/invalid values.</response>
        /// <response code=""500"">There was an error on the server while creating the Product.</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Produces(""application/json"")]
        [HttpDelete(""{{productId}}"")]
        public async Task<ActionResult> DeleteProduct(int productId)
        {{
            // add error handling
            var command = new DeleteProductCommand(productId);
            await _mediator.Send(command);

            return NoContent();
        }}
        
        /// <summary>
        /// Updates an entire existing Product.
        /// </summary>
        /// <response code=""204"">Product updated.</response>
        /// <response code=""400"">Product has missing/invalid values.</response>
        /// <response code=""500"">There was an error on the server while creating the Product.</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Produces(""application/json"")]
        [HttpPut(""{{productId}}"")]
        public async Task<IActionResult> UpdateProduct(int productId, ProductForUpdateDto product)
        {{
            // add error handling
            var command = new UpdateProductCommand(productId, product);
            await _mediator.Send(command);

            return NoContent();
        }}
        
        /// <summary>
        /// Updates specific properties on an existing Product.
        /// </summary>
        /// <response code=""204"">Product updated.</response>
        /// <response code=""400"">Product has missing/invalid values.</response>
        /// <response code=""500"">There was an error on the server while creating the Product.</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Consumes(""application/json"")]
        [Produces(""application/json"")]
        [HttpPatch(""{{productId}}"")]
        public async Task<IActionResult> PartiallyUpdateProduct(int productId, JsonPatchDocument<ProductForUpdateDto> patchDoc)
        {{
            // add error handling
            var command = new PatchProductCommand(productId, patchDoc);
            await _mediator.Send(command);

            return NoContent();
        }}
    }}
}}";

            fileText.Should().Be(expectedText);
        }
    }
}
