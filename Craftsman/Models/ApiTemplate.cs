using System.Collections.Generic;
using System.Linq;

namespace Craftsman.Models
{
    using System.Net;
    using System.Net.Sockets;
    using Humanizer;

    /// <summary>
    /// This is the complete object representation of the API that we will read in from our input file and scaffold out the necessary files
    /// </summary>
    public class ApiTemplate
    {
        private Bus _bus = new();

        /// <summary>
        /// The name of the project in your bounded context
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// The name of the solution you want to build
        /// </summary>
        public TemplateDbContext DbContext { get; set; } = new TemplateDbContext();

        /// <summary>
        /// Complete list of database entities
        /// </summary>
        public List<Entity> Entities { get; set; } = new List<Entity>();

        /// <summary>
        /// Layout of the swagger configuration for the API. Optional
        /// </summary>
        public SwaggerConfig SwaggerConfig { get; set; } = new SwaggerConfig();

        /// <summary>
        /// List of each environment to add into the API. Optional
        /// </summary>
        public ApiEnvironment Environment { get; set; } = new ApiEnvironment();

        /// <summary>
        /// The port that will be used when running locally in the project.
        /// </summary>
        public int Port { get; set; } = 5000;

        /// <summary>
        /// Calculation to determine whether or not authentication is added to the project
        /// </summary>
        public bool AddJwtAuthentication => Environment?.AuthSettings?.Authority?.Length > 0;

        /// <summary>
        /// Message bus information for the bounded context. **Environment will be overriden by the BC environment and should be set there**
        /// </summary>
        public Bus Bus
        {
            get
            {
                _bus.Environment = Environment; // get bus environment settings from domain environments for a single source of truth
                return _bus;
            }
            set => _bus = value;
        }

        /// <summary>
        /// A list of eventing consumers to be added to the BC
        /// </summary>
        public List<Consumer> Consumers { get; set; } = new List<Consumer>();

        /// <summary>
        /// A list of eventing producers to be added to the BC
        /// </summary>
        public List<Producer> Producers { get; set; } = new List<Producer>();

        /// <summary>
        /// The value used for setting the policy name in your swagger config to be used for the scope that has access to the given boundary.
        /// </summary>
        private string _policyName;
        public string PolicyName
        {
            get => _policyName ?? ProjectName.Underscore();
            set => _policyName = value;
        }

        public bool UseSoftDelete { get; set; } = true;

        public DockerConfig DockerConfig { get; set; } = new DockerConfig();
    }
}
