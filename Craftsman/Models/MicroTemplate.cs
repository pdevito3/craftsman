namespace Craftsman.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// This is the complete object representation of the API that we will read in from our input file and scaffold out the necessary files
    /// </summary>
    public class MicroTemplate
    {
        /// <summary>
        /// The name of the solution you want to build
        /// </summary>
        public string SolutionName { get; set; }

        /// <summary>
        /// A list of microservices to add to your solution
        /// </summary>
        public List<Microservice> Microservices { get; set; }

        /// <summary>
        /// Boolean that determines whether or not craftsman will do an initial git set up for this project.
        /// </summary>
        public bool AddGit { get; set; } = true;
    }
}
