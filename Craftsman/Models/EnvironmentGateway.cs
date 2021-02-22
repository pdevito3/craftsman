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
        /// The name of the profile in launch settings for this env.
        /// </summary>
        public string ProfileName { get; set; }

        /// <summary>
        /// The base path for your gateway. (e.g. https://localhost:5050/) used in launchsettings and teh gateway appsettings. 
        /// </summary>
        public string GatewayUrl { get; set; }

        public List<GatewayResource> GatewayResources { get; set; }
    }
}
