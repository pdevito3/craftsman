namespace Craftsman.Models
{
    using System;
    using System.Collections.Generic;

    public class ApplicationUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public List<string> AuthorizedRoles { get; set; }
        public string SeederName { get; set; }
    }
}
