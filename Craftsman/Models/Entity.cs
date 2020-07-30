using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Craftsman.Models
{
    /// <summary>
    /// This represents a database entity for your project
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// This is the directory we want to write the entity class to *relative to the project*
        /// </summary>
        //public string ClassDirectory { get; set; }

        /// <summary>
        /// The name of the entity
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of properties associated to the entity
        /// </summary>
        public List<EntityProperty> Properties { get; set; } = new List<EntityProperty>();
    }
}
