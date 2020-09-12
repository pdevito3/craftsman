namespace Craftsman.Models
{
    using System;
    using System.Collections.Generic;

    public class AuthSetup
    {
        public string AuthMethod { get; set; }
        public List<string> Roles { get; set; }
        public List<ApplicationUser> InMemoryUsers { get; set; }
    }
}
