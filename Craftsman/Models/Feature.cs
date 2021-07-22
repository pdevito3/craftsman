namespace Craftsman.Models
{
    using System;
    using Enums;
    using Exceptions;
    using Helpers;

    public class Feature
    {
        private string _responseType = "bool";
        private FeatureType _featureType;

        public string Type
        {
            get => Enum.GetName(typeof(FeatureType), _featureType);
            set
            {
                if (!Enum.TryParse<FeatureType>(value, true, out var parsed))
                {
                    throw new InvalidFeatureTypeException(value);
                }
                _featureType = parsed;
            }
        }
        
        public string Name { get; set; }
        
        public string Command { get; set; }

        public string ResponseType
        {
            get => _responseType;
            set => _responseType = Utilities.PropTypeCleanup(value);
        }

        public string Directory { get; set; }
        
        // feature role as command, producer, consumer in the future... dropped the ball on the OG implementation
    }
}