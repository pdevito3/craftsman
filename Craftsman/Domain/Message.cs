namespace Craftsman.Domain
{
    using System.Collections.Generic;
    using Helpers;

    public class Message
    {
        private string _name;

        /// <summary>
        /// The name of the message
        /// </summary>
        public string Name
        {
            get => _name ?? Name.UppercaseFirstLetter();
            set => _name = value;
        }

        /// <summary>
        /// List of properties associated to the message
        /// </summary>
        public List<MessageProperty> Properties { get; set; } = new List<MessageProperty>();
    }
}