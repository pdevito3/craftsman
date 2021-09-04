namespace Craftsman.Models
{
    using System;
    using System.Collections.Generic;
    using Enums;
    using Exceptions;
    using Helpers;
    
    public class Feature
    {
        public string EntityName { get; set; }

        public string EntityPlural { get; set; }
        
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
        
        public List<Policy> Policies { get; set; } = new List<Policy>();

        /// <summary>
        /// The name of the property on the foreign entity you are doing a batch add on.
        /// </summary>
        public string BatchPropertyName { get; set; }

        private string _batchPropType;
        /// <summary>
        /// The data type of the the property you are doing the batch add on. This is for a FK property, so probably a Guid or int.
        /// </summary>
        public string BatchPropertyType
        {
            get => _batchPropType;
            set => _batchPropType = Utilities.PropTypeCleanup(value);
        }
        
        /// <summary>
        /// The name of the DbSet for the FK you are doing a batch add on. Generally, the plural of the FK entity. Leave null if you're not batching on a FK.
        /// </summary>
        public string BatchPropertyDbSetName { get; set; }

        // feature role as command, producer, consumer in the future... dropped the ball on the OG implementation
    }
}
