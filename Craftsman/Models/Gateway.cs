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


        //********************************* move below to GatewayEnvironments?

        public List<EnvironmentGateway> EnvironmentGateways { get; set; }

    }
}
