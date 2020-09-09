namespace Craftsman.Models
{
    using System;
    using System.Collections.Generic;

    public class ApiEnvironment
    {
        public string EnvironmentName { get; set; }
        public string ConnectionString { get; set; }
        public string ProfileName { get; set; }
        public JwtSettings JwtSettings { get; set; } = new JwtSettings();
        public MailSettings MailSettings { get; set; } = new MailSettings();
    }
}
