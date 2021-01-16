using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Craftsman.Models
{
    public abstract class TemplateBase
    {
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
        /// The authentication setup information. Optional
        /// </summary>
        public AuthSetup AuthSetup { get; set; } = new AuthSetup();

    }
}
