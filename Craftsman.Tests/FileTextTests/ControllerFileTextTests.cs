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
    using AutoMapper;
    using FluentValidation.AspNetCore;
    using Application.Dtos.Product;
    using Application.Interfaces.Product;
    using Application.Validation.Product;
    using Domain.Entities;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;
    using Application.Wrappers;

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
        
        [Consumes(""application/json"")]
        [Produces(""application/json"")]
        [HttpGet(Name = ""GetProducts"")]
        public async Task<IActionResult> GetProducts([FromQuery] ProductParametersDto productParametersDto)
        {{
            var productsFromRepo = await _productRepository.GetProductsAsync(productParametersDto);

            var paginationMetadata = new
            {{
                totalCount = productsFromRepo.TotalCount,
                pageSize = productsFromRepo.PageSize,
                currentPageSize = productsFromRepo.CurrentPageSize,
                currentStartIndex = productsFromRepo.CurrentStartIndex,
                currentEndIndex = productsFromRepo.CurrentEndIndex,
                pageNumber = productsFromRepo.PageNumber,
                totalPages = productsFromRepo.TotalPages,
                hasPrevious = productsFromRepo.HasPrevious,
                hasNext = productsFromRepo.HasNext
            }};

            Response.Headers.Add(""X-Pagination"",
                JsonSerializer.Serialize(paginationMetadata));

            var productsDto = _mapper.Map<IEnumerable<ProductDto>>(productsFromRepo);
            var response = new Response<IEnumerable<ProductDto>>(productsDto);

            return Ok(response);
        }}
        
        [Produces(""application/json"")]
        [HttpGet(""{{productId}}"", Name = ""GetProduct"")]
        public async Task<ActionResult<ProductDto>> GetProduct(int productId)
        {{
            var productFromRepo = await _productRepository.GetProductAsync(productId);

            if (productFromRepo == null)
            {{
                return NotFound();
            }}

            var productDto = _mapper.Map<ProductDto>(productFromRepo);
            var response = new Response<ProductDto>(productDto);

            return Ok(response);
        }}
        
        [Consumes(""application/json"")]
        [Produces(""application/json"")]
        [HttpPost]
        public async Task<ActionResult<ProductDto>> AddProduct([FromBody]ProductForCreationDto productForCreation)
        {{
            var validationResults = new ProductForCreationDtoValidator().Validate(productForCreation);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {{
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }}

            var product = _mapper.Map<Product>(productForCreation);
            await _productRepository.AddProduct(product);
            var saveSuccessful = await _productRepository.SaveAsync();

            if(saveSuccessful)
            {{
                var productFromRepo = await _productRepository.GetProductAsync(product.ProductId);
                var productDto = _mapper.Map<ProductDto>(productFromRepo);
                var response = new Response<ProductDto>(productDto);
                
                return CreatedAtRoute(""GetProduct"",
                    new {{ productDto.ProductId }},
                    response);
            }}

            return StatusCode(500);
        }}
        
        [Produces(""application/json"")]
        [HttpDelete(""{{productId}}"")]
        public async Task<ActionResult> DeleteProduct(int productId)
        {{
            var productFromRepo = await _productRepository.GetProductAsync(productId);

            if (productFromRepo == null)
            {{
                return NotFound();
            }}

            _productRepository.DeleteProduct(productFromRepo);
            await _productRepository.SaveAsync();

            return NoContent();
        }}
        
        [Produces(""application/json"")]
        [HttpPut(""{{productId}}"")]
        public async Task<IActionResult> UpdateProduct(int productId, ProductForUpdateDto product)
        {{
            var productFromRepo = await _productRepository.GetProductAsync(productId);

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

            await _productRepository.SaveAsync();

            return NoContent();
        }}
        
        [Consumes(""application/json"")]
        [Produces(""application/json"")]
        [HttpPatch(""{{productId}}"")]
        public async Task<IActionResult> PartiallyUpdateProduct(int productId, JsonPatchDocument<ProductForUpdateDto> patchDoc)
        {{
            if (patchDoc == null)
            {{
                return BadRequest();
            }}

            var existingProduct = await _productRepository.GetProductAsync(productId);

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

            await _productRepository.SaveAsync(); // save changes in the database

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
    using AutoMapper;
    using FluentValidation.AspNetCore;
    using Application.Dtos.Buffalo;
    using Application.Interfaces.Buffalo;
    using Application.Validation.Buffalo;
    using Domain.Entities;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;
    using Application.Wrappers;

    [ApiController]
    [Route(""api/Buffalo"")]
    [ApiVersion(""1.0"")]
    public class BuffaloController: Controller
    {{
        private readonly IBuffaloRepository _buffaloRepository;
        private readonly IMapper _mapper;

        public BuffaloController(IBuffaloRepository buffaloRepository
            , IMapper mapper)
        {{
            _buffaloRepository = buffaloRepository ??
                throw new ArgumentNullException(nameof(buffaloRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }}
        
        [Consumes(""application/json"")]
        [Produces(""application/json"")]
        [HttpGet(Name = ""GetBuffaloList"")]
        public async Task<IActionResult> GetBuffalo([FromQuery] BuffaloParametersDto buffaloParametersDto)
        {{
            var buffalosFromRepo = await _buffaloRepository.GetBuffaloAsync(buffaloParametersDto);

            var paginationMetadata = new
            {{
                totalCount = buffalosFromRepo.TotalCount,
                pageSize = buffalosFromRepo.PageSize,
                currentPageSize = buffalosFromRepo.CurrentPageSize,
                currentStartIndex = buffalosFromRepo.CurrentStartIndex,
                currentEndIndex = buffalosFromRepo.CurrentEndIndex,
                pageNumber = buffalosFromRepo.PageNumber,
                totalPages = buffalosFromRepo.TotalPages,
                hasPrevious = buffalosFromRepo.HasPrevious,
                hasNext = buffalosFromRepo.HasNext
            }};

            Response.Headers.Add(""X-Pagination"",
                JsonSerializer.Serialize(paginationMetadata));

            var buffaloDto = _mapper.Map<IEnumerable<BuffaloDto>>(buffalosFromRepo);
            var response = new Response<IEnumerable<BuffaloDto>>(buffaloDto);

            return Ok(response);
        }}
        
        [Produces(""application/json"")]
        [HttpGet(""{{buffaloId}}"", Name = ""GetBuffaloRecord"")]
        public async Task<ActionResult<BuffaloDto>> GetBuffalo(int buffaloId)
        {{
            var buffaloFromRepo = await _buffaloRepository.GetBuffaloAsync(buffaloId);

            if (buffaloFromRepo == null)
            {{
                return NotFound();
            }}

            var buffaloDto = _mapper.Map<BuffaloDto>(buffaloFromRepo);
            var response = new Response<BuffaloDto>(buffaloDto);

            return Ok(response);
        }}
        
        [Consumes(""application/json"")]
        [Produces(""application/json"")]
        [HttpPost]
        public async Task<ActionResult<BuffaloDto>> AddBuffalo([FromBody]BuffaloForCreationDto buffaloForCreation)
        {{
            var validationResults = new BuffaloForCreationDtoValidator().Validate(buffaloForCreation);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {{
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }}

            var buffalo = _mapper.Map<Buffalo>(buffaloForCreation);
            await _buffaloRepository.AddBuffalo(buffalo);
            var saveSuccessful = await _buffaloRepository.SaveAsync();

            if(saveSuccessful)
            {{
                var buffaloFromRepo = await _buffaloRepository.GetBuffaloAsync(buffalo.ProductId);
                var buffaloDto = _mapper.Map<BuffaloDto>(buffaloFromRepo);
                var response = new Response<BuffaloDto>(buffaloDto);
                
                return CreatedAtRoute(""GetBuffalo"",
                    new {{ buffaloDto.ProductId }},
                    response);
            }}

            return StatusCode(500);
        }}
        
        [Produces(""application/json"")]
        [HttpDelete(""{{buffaloId}}"")]
        public async Task<ActionResult> DeleteBuffalo(int buffaloId)
        {{
            var buffaloFromRepo = await _buffaloRepository.GetBuffaloAsync(buffaloId);

            if (buffaloFromRepo == null)
            {{
                return NotFound();
            }}

            _buffaloRepository.DeleteBuffalo(buffaloFromRepo);
            await _buffaloRepository.SaveAsync();

            return NoContent();
        }}
        
        [Produces(""application/json"")]
        [HttpPut(""{{buffaloId}}"")]
        public async Task<IActionResult> UpdateBuffalo(int buffaloId, BuffaloForUpdateDto buffalo)
        {{
            var buffaloFromRepo = await _buffaloRepository.GetBuffaloAsync(buffaloId);

            if (buffaloFromRepo == null)
            {{
                return NotFound();
            }}

            var validationResults = new BuffaloForUpdateDtoValidator().Validate(buffalo);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {{
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }}

            _mapper.Map(buffalo, buffaloFromRepo);
            _buffaloRepository.UpdateBuffalo(buffaloFromRepo);

            await _buffaloRepository.SaveAsync();

            return NoContent();
        }}
        
        [Consumes(""application/json"")]
        [Produces(""application/json"")]
        [HttpPatch(""{{buffaloId}}"")]
        public async Task<IActionResult> PartiallyUpdateBuffalo(int buffaloId, JsonPatchDocument<BuffaloForUpdateDto> patchDoc)
        {{
            if (patchDoc == null)
            {{
                return BadRequest();
            }}

            var existingBuffalo = await _buffaloRepository.GetBuffaloAsync(buffaloId);

            if (existingBuffalo == null)
            {{
                return NotFound();
            }}

            var buffaloToPatch = _mapper.Map<BuffaloForUpdateDto>(existingBuffalo); // map the buffalo we got from the database to an updatable buffalo model
            patchDoc.ApplyTo(buffaloToPatch, ModelState); // apply patchdoc updates to the updatable buffalo

            if (!TryValidateModel(buffaloToPatch))
            {{
                return ValidationProblem(ModelState);
            }}

            _mapper.Map(buffaloToPatch, existingBuffalo); // apply updates from the updatable buffalo to the db entity so we can apply the updates to the database
            _buffaloRepository.UpdateBuffalo(existingBuffalo); // apply business updates to data if needed

            await _buffaloRepository.SaveAsync(); // save changes in the database

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
    using AutoMapper;
    using FluentValidation.AspNetCore;
    using Application.Dtos.Product;
    using Application.Interfaces.Product;
    using Application.Validation.Product;
    using Domain.Entities;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;
    using Application.Wrappers;

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
            var productsFromRepo = await _productRepository.GetProductsAsync(productParametersDto);

            var paginationMetadata = new
            {{
                totalCount = productsFromRepo.TotalCount,
                pageSize = productsFromRepo.PageSize,
                currentPageSize = productsFromRepo.CurrentPageSize,
                currentStartIndex = productsFromRepo.CurrentStartIndex,
                currentEndIndex = productsFromRepo.CurrentEndIndex,
                pageNumber = productsFromRepo.PageNumber,
                totalPages = productsFromRepo.TotalPages,
                hasPrevious = productsFromRepo.HasPrevious,
                hasNext = productsFromRepo.HasNext
            }};

            Response.Headers.Add(""X-Pagination"",
                JsonSerializer.Serialize(paginationMetadata));

            var productsDto = _mapper.Map<IEnumerable<ProductDto>>(productsFromRepo);
            var response = new Response<IEnumerable<ProductDto>>(productsDto);

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
            var productFromRepo = await _productRepository.GetProductAsync(productId);

            if (productFromRepo == null)
            {{
                return NotFound();
            }}

            var productDto = _mapper.Map<ProductDto>(productFromRepo);
            var response = new Response<ProductDto>(productDto);

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
            var validationResults = new ProductForCreationDtoValidator().Validate(productForCreation);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {{
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }}

            var product = _mapper.Map<Product>(productForCreation);
            await _productRepository.AddProduct(product);
            var saveSuccessful = await _productRepository.SaveAsync();

            if(saveSuccessful)
            {{
                var productFromRepo = await _productRepository.GetProductAsync(product.ProductId);
                var productDto = _mapper.Map<ProductDto>(productFromRepo);
                var response = new Response<ProductDto>(productDto);
                
                return CreatedAtRoute(""GetProduct"",
                    new {{ productDto.ProductId }},
                    response);
            }}

            return StatusCode(500);
        }}
        
        /// <summary>
        /// Deletes an existing Product record.
        /// </summary>
        /// <response code=""201"">Product deleted.</response>
        /// <response code=""400"">Product has missing/invalid values.</response>
        /// <response code=""500"">There was an error on the server while creating the Product.</response>
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Produces(""application/json"")]
        [HttpDelete(""{{productId}}"")]
        public async Task<ActionResult> DeleteProduct(int productId)
        {{
            var productFromRepo = await _productRepository.GetProductAsync(productId);

            if (productFromRepo == null)
            {{
                return NotFound();
            }}

            _productRepository.DeleteProduct(productFromRepo);
            await _productRepository.SaveAsync();

            return NoContent();
        }}
        
        /// <summary>
        /// Updates an entire existing Product.
        /// </summary>
        /// <response code=""201"">Product updated.</response>
        /// <response code=""400"">Product has missing/invalid values.</response>
        /// <response code=""500"">There was an error on the server while creating the Product.</response>
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Produces(""application/json"")]
        [HttpPut(""{{productId}}"")]
        public async Task<IActionResult> UpdateProduct(int productId, ProductForUpdateDto product)
        {{
            var productFromRepo = await _productRepository.GetProductAsync(productId);

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

            await _productRepository.SaveAsync();

            return NoContent();
        }}
        
        /// <summary>
        /// Updates specific properties on an existing Product.
        /// </summary>
        /// <response code=""201"">Product updated.</response>
        /// <response code=""400"">Product has missing/invalid values.</response>
        /// <response code=""500"">There was an error on the server while creating the Product.</response>
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        [Consumes(""application/json"")]
        [Produces(""application/json"")]
        [HttpPatch(""{{productId}}"")]
        public async Task<IActionResult> PartiallyUpdateProduct(int productId, JsonPatchDocument<ProductForUpdateDto> patchDoc)
        {{
            if (patchDoc == null)
            {{
                return BadRequest();
            }}

            var existingProduct = await _productRepository.GetProductAsync(productId);

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

            await _productRepository.SaveAsync(); // save changes in the database

            return NoContent();
        }}
    }}
}}";

            fileText.Should().Be(expectedText);
        }
    }
}
