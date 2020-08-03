using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Craftsman.Models
{
    /// <summary>
    /// This represents a database entity for your project
    /// </summary>
    public class Entity
    {
        private string _plural;

        /// <summary>
        /// The name of the entity
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Plural name of the entity. Will default to the entity name with an 's' at the end.
        /// </summary>
        public string Plural {
            get => _plural ?? $"{Name}s";
            set => _plural = value;
        }

        /// <summary>
        /// List of properties associated to the entity
        /// </summary>
        public List<EntityProperty> Properties { get; set; } = new List<EntityProperty>();
    }
}
