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
        /// Complete list of database entities
        /// </summary>
        public List<Entity> Entities { get; set; }
    }
}
