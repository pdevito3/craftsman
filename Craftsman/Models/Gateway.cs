namespace Craftsman.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Gateway
    {
        public string GatewayProjectName { get; set; }

        public List<EnvironmentGateway> EnvironmentGateways { get; set; } = new List<EnvironmentGateway>();

    }
}
