namespace Craftsman.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class EnvironmentGateway
    {
        /// <summary>
        /// The name of the environment to match ASPNET Environment. Same as what's tagged on Startup and app settings.
        /// </summary>
        public string EnvironmentName { get; set; }

        /// <summary>
        /// The base path for your gateway. (e.g. https://localhost:5050/api)
        /// </summary>
        public string GatewayUrl { get; set; }

        public List<GatewayTemplate> GatewayTemplates { get; set; }
    }
}
