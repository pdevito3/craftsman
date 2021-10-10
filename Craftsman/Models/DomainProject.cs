namespace Craftsman.Models
{
    using System.Collections.Generic;

    public class DomainProject
    {
        public string DomainName { get; set; }

        public List<ApiTemplate> BoundedContexts { get; set; }

        public bool AddGit { get; set; } = true;

        /// <summary>
        /// A list of eventing messages to be added to the domain
        /// </summary>
        public List<Message> Messages { get; set; } = new List<Message>();

        public AuthServerTemplate AuthServer { get; set; } = null;
    }
}