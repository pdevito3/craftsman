namespace Craftsman.Models
{
    using System;
    using Enums;
    using Exceptions;
    using Helpers;

    public class Feature
    {
        public string EntityName { get; set; }
        
        private FeatureType FeatureType { get; set; }
        public string Type
        {
            get => FeatureType.Name;
            set
            {
                if (!FeatureType.TryFromName(value, true, out var parsed))
                {
                    if(value.Equals("CreateRecord", StringComparison.InvariantCultureIgnoreCase))
                        FeatureType = FeatureType.AddRecord;
                    else
                        throw new InvalidFeatureTypeException(value);
                }
                FeatureType = parsed;
            }
        }

        private string _name = null;
        public string Name
        {
            get => FeatureType.FeatureName(_name);
            set => _name = value;
        }

        private string _command = null;
        public string Command
        {
            get => FeatureType.CommandName(_command, EntityName);
            set => _command = value;
        }

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