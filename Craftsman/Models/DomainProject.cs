namespace Craftsman.Models
{
    using System.Collections.Generic;

    public class DomainProject
    {
        public string DomainName { get; set; }

        public List<ApiTemplate> BoundedContexts { get; set; }

        public bool AddGit { get; set; } = true;
    }
}
