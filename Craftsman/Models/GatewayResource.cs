namespace Craftsman.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class GatewayResource
    {
        /// <summary>
        /// This is the path template for the GATEWAY url (e.g. /recipes)
        /// </summary>
        public string GatewayRoute { get; set; }

        /// <summary>
        /// The name of the entity that will be called downstream
        /// </summary>
        public string DownstreamEntityName { get; set; }

        ///// <summary>
        ///// This is the path template for the SOURCE (non gateway) url (e.g. api/recipes)
        ///// </summary>
        //public string DownstreamPathTemplate { get; set; }

        ///// <summary>
        ///// This is the port for the SOURCE (non gateway) url.
        ///// </summary>
        //public string DownstreamPort { get; set; }

        // below two props so they can chose to build an envoy gateway or an ocelot gateway. 
        // gateways can be segregated for bff (backend for frontend) or business boundaries for a domain driven approach also work.
        // prop: envoy config
        // prop: ocelot config

        // auto add all methods for upstreamhttpmethod. not configurable
        // auth provider key?
        // delegation handler?
    }
}
