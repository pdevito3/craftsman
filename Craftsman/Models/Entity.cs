using System.Collections.Generic;
using System.Linq;

namespace Craftsman.Models
{
    /// <summary>
    /// This represents a database entity for your project
    /// </summary>
    public class Entity
    {
        private string _plural;
        private string _lambda;
        private string _name;

        /// <summary>
        /// The name of the entity
        /// </summary>
        public string Name
        {
            get => _name.UppercaseFirstLetter();
            set => _name = value;
        }

        /// <summary>
        /// List of properties associated to the entity
        /// </summary>
        public List<EntityProperty> Properties { get; set; } = new List<EntityProperty>();

        /// <summary>
        /// The Plural name of the entity.
        /// </summary>
        public string Plural
        {
            get => _plural ?? $"{Name}s";
            set => _plural = value;
        }

        /// <summary>
        /// The value to use in lambda expressions for this entity. Will default to the first letter of the entity name if none is given.
        /// </summary>
        public string Lambda
        {
            get => _lambda ?? Name.Substring(0, 1).ToLower();
            set => _lambda = value;
        }

        /// <summary>
        /// The properties that are set to be a key. List of properties in case there is a composite key.
        /// </summary>
        public EntityProperty PrimaryKeyProperty
        {
            get => Properties.Where(p => p.IsPrimaryKey).FirstOrDefault();
        }

        /// <summary>
        /// The custom table name that will be used in the database. Optional and null if they want to use default value.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// The database schema that will be used. Optional and null if they want to use default value
        /// </summary>
        public string Schema { get; set; }

        public List<Feature> Features { get; set; }
    }
}