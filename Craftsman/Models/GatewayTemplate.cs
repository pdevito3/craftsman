namespace Craftsman.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class GatewayTemplate
    {
        /// <summary>
        /// This is the path template for the GATEWAY url (e.g. /recipes)
        /// </summary>
        public string UpstreamPathTemplate { get; set; }

        /// <summary>
        /// This is the path template for the SOURCE (non gateway) url (e.g. api/recipes)
        /// </summary>
        public string DownstreamPathTemplate { get; set; }

        // below two props so they can chose to build an envoy gateway or an ocelot gateway. 
        // gateways can be segregated for bff (backend for frontend) or business boundaries for a domain driven approach also work.
        // prop: envoy config
        // prop: ocelot config

        // auto add all methods for upstreamhttpmethod. not configurable
        // auth provider key?
        // delegation handler?
    }
}
