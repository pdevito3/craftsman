namespace NewCraftsman.Domain
{
    public class ApiEnvironment
    {
        public string EnvironmentName => "Development";

        public string ProfileName { get; set; }

        public AuthSettings AuthSettings { get; set; } = new AuthSettings();
        public BrokerSettings BrokerSettings { get; set; } = new BrokerSettings();
    }
}