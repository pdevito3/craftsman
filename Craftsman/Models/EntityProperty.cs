using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Craftsman.Models
{
    public class EntityProperty
    {
        private bool _isRequired;
        private bool _canManipulate;

        /// <summary>
        /// Name of the property
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of property (e.g. string, int, DateTime?, etc.)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Determines if the property will be filterable by the API
        /// </summary>
        public bool CanFilter { get; set; } = false;

        /// <summary>
        /// Determines if the property will be sortable by the API
        /// </summary>
        public bool CanSort { get; set; } = false;

        //TODO update to default to true unless primary key == true
        /// <summary>
        /// Determines if the property can be manipulated when creating or updating the associated entity
        /// </summary>
        public bool CanManipulate
        {
            get => _canManipulate;
            set
            {
                if (IsPrimaryKey)
                    _canManipulate = true;

                _canManipulate = value;
            }
        }

        /// <summary>
        /// Designates the property as the primary key for the entity
        /// </summary>
        public bool IsPrimaryKey { get; set; } = false;

        /// <summary>
        /// Denotes a required field in the database
        /// </summary>
        public bool IsRequired
        {
            get => _isRequired;
            set
            {
                if (IsPrimaryKey)
                    _isRequired = true;

                _isRequired = value;
            }
        }
    }
}
