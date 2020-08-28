namespace Craftsman.Tests.Fakes
{
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class CannedGenerator
    {
        public static Entity FakeBasicProduct()
        {
            return new Entity()
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
        }

        public static ApiTemplate FakeBasicApiTemplate()
        {
            return new ApiTemplate()
            {
                SolutionName = "BespokedBikes.Api",
                DbContext = new TemplateDbContext()
                {
                    ContextName = "BespokedBikesDbContext",
                    DatabaseName = "BespokedBikes",
                    Provider = "SqlServer"
                },
                Entities = new List<Entity>()
                {
                    FakeBasicProduct()
                }
            };
        }
    }
}
