namespace NewCraftsman.Domain
{
    using System.Collections.Generic;

    /// <summary>
    /// This is the complete object representation of the API that we will read in from our input file and scaffold out the necessary files
    /// </summary>
    public class AddEntityTemplate
    {
        /// <summary>
        /// The name of the solution you want to build
        /// </summary>
        public string SolutionName { get; set; }

        /// <summary>
        /// The name of the solution you want to build
        /// </summary>
        public string DbContextName { get; set; }

        /// <summary>
        /// Complete list of database entities
        /// </summary>
        public List<Entity> Entities { get; set; } = new List<Entity>();

        /// <summary>
        /// Add swagger comments to the controller Optional
        /// </summary>
        public bool AddSwaggerComments { get; set; } = true;
    }
}
