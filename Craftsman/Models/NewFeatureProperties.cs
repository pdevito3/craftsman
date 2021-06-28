namespace Craftsman.Models
{
    using Craftsman.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class NewFeatureProperties
    {
        private string _type = "bool";

        public string Feature { get; set; }
        public string Command { get; set; }

        public string ResponseType
        {
            get => _type;
            set => _type = Utilities.PropTypeCleanup(value);
        }

        public string Directory { get; set; }

        public bool IsProducer { get; set; } = false;
    }
}