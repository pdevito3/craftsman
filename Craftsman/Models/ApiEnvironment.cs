namespace Craftsman.Models
{
    using System;

    public class ApiEnvironment
    {
        public string EnvironmentName => "Development";

        private string _profileName;
        public string ProfileName 
        { 
            get => _profileName;
            set => _profileName = value ?? EnvironmentName;
        }

        public AuthSettings AuthSettings { get; set; } = new AuthSettings();
        public BrokerSettings BrokerSettings { get; set; } = new BrokerSettings();
    }
}