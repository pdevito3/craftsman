using System.Collections.Generic;

namespace Craftsman.Models
{
    public class AuthorizationSettings
    {
        public List<Policy> Policies { get; set; } = new List<Policy>();
    }
}
