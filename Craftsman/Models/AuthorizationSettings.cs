using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Craftsman.Models
{
    public class AuthorizationSettings
    {
        public List<Policy> Policies { get; set; } = new List<Policy>();
    }
}
