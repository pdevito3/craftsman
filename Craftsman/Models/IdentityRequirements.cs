namespace Craftsman.Models
{
    using System;
    using System.Collections.Generic;

    public class IdentityRequirements
    {
        public int RequiredLength { get; set; } = 12;
        public bool RequireDigit { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireNonAlphanumeric { get; set; } = false;
        public bool RequireUniqueEmail { get; set; } = true;
    }
}
