namespace Craftsman.Models
{
    using System.Collections.Generic;
    using System.Linq;

    public class Microservice
    {
        /// <summary>
        /// The base name of the projects in your microservice (e.g. Ordering -> Ordering.Domain, Ordering.Application, etc.).
        /// </summary>
        public string ProjectFolderName { get; set; }

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
        /// The port that will be used when running locally in the project (and referenced by the gateway).
        /// </summary>
        public int Port { get; set; } = 5000;

        public bool AddJwtAuthentication
        {
            get => Environments
                    .Where(e => e.Authority.Length > 0)
                    .ToList()
                    .Count > 0;
        }

        public AuthorizationSettings AuthorizationSettings { get; set; }
    }
}
