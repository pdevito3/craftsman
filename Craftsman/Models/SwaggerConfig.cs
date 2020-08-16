namespace Craftsman.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;

    public class SwaggerConfig
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public SwaggerApiContact ApiContact { get; set; } = new SwaggerApiContact();
        //public SwaggerUi SwaggerUi { get; set; } = new SwaggerUi();
    }
}
