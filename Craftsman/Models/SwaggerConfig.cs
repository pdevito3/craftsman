namespace Craftsman.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;

    public class SwaggerConfig
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string SwaggerEndpointUrl { get; set; } = "/swagger/v1/swagger.json";
        public string SwaggerEndpointName { get; set; }
        public SwaggerApiContact ApiContact { get; set; } = new SwaggerApiContact();
        public string LicenseName { get; set; }
        public string LicenseUrl { get; set; }
    }
}
