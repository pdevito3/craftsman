namespace Craftsman.Tests.FileTextTests
{
    using AutoBogus;
    using Craftsman.Builders;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using Craftsman.Tests.Fakes;
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class RepositoryTextFileTests
    {
        [Fact]
        public void GetIRepositoryFileText_passed_normal_product_entity_creates_expected_text()
        {
            var classNamespace = "Application.Interfaces.Product";
            var entity = new FakeEntity().Generate();
            entity.Name = "Product";
            entity.Plural = "Products";

            var fileText = RepositoryBuilder.GetIRepositoryFileText(classNamespace, entity);

            var expectedText = @"namespace Application.Interfaces.Product
{
    using Application.Dtos.Product;
    using Application.Wrappers;
    using System.Threading.Tasks;
    using Domain.Entities;

    public interface IProductRepository
    {
        Task<PagedList<Product>> GetProductsAsync(ProductParametersDto ProductParameters);
        Task<Product> GetProductAsync(int ProductId);
        Product GetProduct(int ProductId);
        Task AddProduct(Product product);
        void DeleteProduct(Product product);
        void UpdateProduct(Product product);
        bool Save();
    }
}";

            fileText.Should().Be(expectedText);
        }

        [Fact]
        public void GetRepositoryFileText_passed_normal_product_entity_without_FK_creates_expected_text()
        {
            var classNamespace = "Infrastructure.Persistence.Repositories";
            var entity = new Entity()
            {
                Name = "Product",
                Properties = new List<EntityProperty>()
                {
                    new EntityProperty()
                    {
                        Name = "ProductId",
                        Type = "int",
                        IsPrimaryKey = true,
                        CanFilter = true,
                        CanSort = false,
                    },
                    new EntityProperty()
                    {
                        Name = "Name",
                        Type = "string",
                        CanFilter = true,
                        CanSort = false,
                    },
                }
            };

            var context = new AutoFaker<TemplateDbContext>().Generate();
            context.ContextName = "BespokedBikesDbContext";

            var fileText = RepositoryBuilder.GetRepositoryFileText(classNamespace, entity, context);

            var expectedText = @"namespace Infrastructure.Persistence.Repositories
{
    using Application.Dtos.Product;
    using Application.Interfaces.Product;
    using Application.Wrappers;
    using Domain.Entities;
    using Infrastructure.Persistence.Contexts;
    using Microsoft.EntityFrameworkCore;
    using Sieve.Models;
    using Sieve.Services;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class ProductRepository : IProductRepository
    {
        private BespokedBikesDbContext _context;
        private readonly SieveProcessor _sieveProcessor;

        public ProductRepository(BespokedBikesDbContext context,
            SieveProcessor sieveProcessor)
        {
            _context = context
                ?? throw new ArgumentNullException(nameof(context));
            _sieveProcessor = sieveProcessor ??
                throw new ArgumentNullException(nameof(sieveProcessor));
        }

        public async Task<PagedList<Product>> GetProductsAsync(ProductParametersDto productParameters)
        {
            if (productParameters == null)
            {
                throw new ArgumentNullException(nameof(productParameters));
            }

            var collection = _context.Products 
                as IQueryable<Product>; // TODO: AsNoTracking() should increase performance, but will break the sort tests. need to investigate

            var sieveModel = new SieveModel
            {
                Sorts = productParameters.SortOrder,
                Filters = productParameters.Filters
            };

            collection = _sieveProcessor.Apply(sieveModel, collection);

            return await PagedList<Product>.CreateAsync(collection,
                productParameters.PageNumber,
                productParameters.PageSize);
        }

        public async Task<Product> GetProductAsync(int productId)
        {
            // include marker -- requires return _context.Products as it's own line with no extra text -- do not delete this comment
            return await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public Product GetProduct(int productId)
        {
            // include marker -- requires return _context.Products as it's own line with no extra text -- do not delete this comment
            return _context.Products
                .FirstOrDefault(p => p.ProductId == productId);
        }

        public Task AddProduct(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(Product));
            }

            return await _context.Products.AddAsync(product);
        }

        public void DeleteProduct(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(Product));
            }

            _context.Products.Remove(product);
        }

        public void UpdateProduct(Product product)
        {
            // no implementation for now
        }

        public bool Save()
        {
            return _context.SaveChanges() > 0;
        }
    }
}";

            fileText.Should().Be(expectedText);
        }

        [Fact]
        public void GetRepositoryFileText_passed_product_entity_with_FK_creates_expected_text_with_includes()
        {
            var fkObjectName = "Creator";
            var classNamespace = "Infrastructure.Persistence.Repositories";
            var entity = new Entity()
            {
                Name = "Product",
                Properties = new List<EntityProperty>()
                {
                    new EntityProperty()
                    {
                        Name = "ProductId",
                        Type = "int",
                        IsPrimaryKey = true,
                        CanFilter = true,
                        CanSort = false,
                    },
                    new EntityProperty()
                    {
                        Name = "Name",
                        Type = "string",
                        CanFilter = true,
                        CanSort = false,
                    },
                    new EntityProperty()
                    {
                        Name = "CreatorId",
                        Type = "int",
                        CanFilter = true,
                        CanSort = false,
                    },
                    new EntityProperty()
                    {
                        Name = fkObjectName,
                        Type = fkObjectName,
                        ForeignKeyPropName = "CreatorId"
                    },
                }
            };

            var context = new AutoFaker<TemplateDbContext>().Generate();
            context.ContextName = "BespokedBikesDbContext";

            var fileText = RepositoryBuilder.GetRepositoryFileText(classNamespace, entity, context);

            var expectedText = @$"namespace Infrastructure.Persistence.Repositories
{{
    using Application.Dtos.Product;
    using Application.Interfaces.Product;
    using Application.Wrappers;
    using Domain.Entities;
    using Infrastructure.Persistence.Contexts;
    using Microsoft.EntityFrameworkCore;
    using Sieve.Models;
    using Sieve.Services;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class ProductRepository : IProductRepository
    {{
        private BespokedBikesDbContext _context;
        private readonly SieveProcessor _sieveProcessor;

        public ProductRepository(BespokedBikesDbContext context,
            SieveProcessor sieveProcessor)
        {{
            _context = context
                ?? throw new ArgumentNullException(nameof(context));
            _sieveProcessor = sieveProcessor ??
                throw new ArgumentNullException(nameof(sieveProcessor));
        }}

        public async Task<PagedList<Product>> GetProductsAsync(ProductParametersDto productParameters)
        {{
            if (productParameters == null)
            {{
                throw new ArgumentNullException(nameof(productParameters));
            }}

            var collection = _context.Products
                .Include({fkObjectName.ToLower().Substring(0, 1)} => {fkObjectName.ToLower().Substring(0, 1)}.{fkObjectName}) 
                as IQueryable<Product>; // TODO: AsNoTracking() should increase performance, but will break the sort tests. need to investigate

            var sieveModel = new SieveModel
            {{
                Sorts = productParameters.SortOrder,
                Filters = productParameters.Filters
            }};

            collection = _sieveProcessor.Apply(sieveModel, collection);

            return await PagedList<Product>.CreateAsync(collection,
                productParameters.PageNumber,
                productParameters.PageSize);
        }}

        public async Task<Product> GetProductAsync(int productId)
        {{
            // include marker -- requires return _context.Products as it's own line with no extra text -- do not delete this comment
            return await _context.Products
                .Include({fkObjectName.ToLower().Substring(0, 1)} => {fkObjectName.ToLower().Substring(0, 1)}.{fkObjectName})
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }}

        public Product GetProduct(int productId)
        {{
            // include marker -- requires return _context.Products as it's own line with no extra text -- do not delete this comment
            return _context.Products
                .Include({fkObjectName.ToLower().Substring(0, 1)} => {fkObjectName.ToLower().Substring(0, 1)}.{fkObjectName})
                .FirstOrDefault(p => p.ProductId == productId);
        }}

        public Task AddProduct(Product product)
        {{
            if (product == null)
            {{
                throw new ArgumentNullException(nameof(Product));
            }}

            return await _context.Products.AddAsync(product);
        }}

        public void DeleteProduct(Product product)
        {{
            if (product == null)
            {{
                throw new ArgumentNullException(nameof(Product));
            }}

            _context.Products.Remove(product);
        }}

        public void UpdateProduct(Product product)
        {{
            // no implementation for now
        }}

        public bool Save()
        {{
            return _context.SaveChanges() > 0;
        }}
    }}
}}";

            fileText.Should().Be(expectedText);
        }
    }
}
