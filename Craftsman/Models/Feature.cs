namespace Craftsman.Models
{
    using Enums;
    using Helpers;

    public class Feature
    {
        private string _responseType = "bool";
        
        public FeatureType Type { get; set; }
        
        public string Name { get; set; }
        
        public string Command { get; set; }

        public string ResponseType
        {
            get => _responseType;
            set => _responseType = Utilities.PropTypeCleanup(value);
        }

        public string Directory { get; set; }

        public bool IsProducer { get; set; } = false;
    }
}