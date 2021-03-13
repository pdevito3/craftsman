namespace Craftsman.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class DomainProject
    {
        public string DomainName { get; set; }

        public List<ApiTemplate> BoundedContexts { get; set; }

        public bool AddGit { get; set; } = true;
    }
}
