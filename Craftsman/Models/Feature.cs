namespace Craftsman.Models
{
    using System;
    using Enums;
    using Exceptions;
    using Helpers;

    public class Feature
    {
        private FeatureType _featureType;
        public string Type
        {
            get => _featureType.Name;
            set
            {
                if (!FeatureType.TryFromName(value, true, out var parsed))
                {
                    throw new InvalidFeatureTypeException(value);
                }
                _featureType = parsed;
            }
        }

        private readonly string _url = null;
        public string Url 
        {
            get => _url;
            set => _featureType.Url(value);
        }
        
        public string Name { get; set; }
        
        public string Command { get; set; }

        private string _responseType = "bool";
        public string ResponseType
        {
            get => _responseType;
            set => _responseType = Utilities.PropTypeCleanup(value);
        }

        public string Directory { get; set; }
        
        // feature role as command, producer, consumer in the future... dropped the ball on the OG implementation
    }
}