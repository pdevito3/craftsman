namespace Craftsman.Models
{
    using Craftsman.Exceptions;
    using System;
    using System.Collections.Generic;

    public class EntityEndpoint
    {
        private List<Endpoint> _endpoints = new List<Endpoint>();

        public string EntityName { get; set; }

        public List<string> RestrictedEndpoints
        {
            get
            {
                var endpoints = new List<string>();
                foreach (var endpoint in _endpoints)
                    endpoints.Add(Enum.GetName(typeof(Endpoint), endpoint));

                return endpoints;
            }
            set
            {
                _endpoints.Clear();
                foreach (var endpoint in value)
                {
                    if (!Enum.TryParse<Endpoint>(endpoint, true, out var parsed))
                    {
                        throw new InvalidEndpointException(endpoint);
                    }
                    _endpoints.Add(parsed);
                }
            }
        }
    }
}
