namespace Craftsman.Models
{
    using System;
    using System.Collections.Generic;

    public class ApiEnvironment
    {
        private string _environmentName;

        public string EnvironmentName {
            get => _environmentName;
            set
            {
                if (value.Equals("Startup", StringComparison.InvariantCultureIgnoreCase))
                    _environmentName = "Startup";
                else
                    _environmentName = value;
            }
        }

        public string ConnectionString { get; set; }

        public string ProfileName { get; set; }

        public string Authority { get; set; }

        public string Audience { get; set; }
    }
}
