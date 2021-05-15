using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Craftsman.Models
{
    /// <summary>
    /// This is the complete object representation of the API that we will read in from our input file and scaffold out the necessary files
    /// </summary>
    public class ApiTemplate
    {
        /// <summary>
        /// The name of the solution you want to build
        /// </summary>
        public string SolutionName { get; set; }

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
        public List<ApiEnvironment> Environments { get; set; } = new List<ApiEnvironment>();

        /// <summary>
        /// The port that will be used when running locally in the project.
        /// </summary>
        public int Port { get; set; } = 5000;

        /// <summary>
        /// Calculation to determine whether or not authentication is added to the project
        /// </summary>
        public bool AddJwtAuthentication
        {
            get => Environments
                .Where(e => e?.Authority?.Length > 0)
                .ToList()
                .Count > 0;
        }

        /// <summary>
        /// The message broker to be applied to the message bus (if used). just a string here and will be transformed when mapped to `Bus` Broker prop
        /// </summary>
        public string Broker { get; set; }

        /// <summary>
        /// Will trigger the add service bus method and add it to the solution
        /// </summary>
        public bool AddBus { get; set; }

        public AuthorizationSettings AuthorizationSettings { get; set; } = new AuthorizationSettings();
    }
}