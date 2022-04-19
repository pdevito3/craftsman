namespace NewCraftsman.Domain
{
    using System;
    using Enums;
    using Exceptions;

    public class Bus
    {
        private MessageBroker _broker = MessageBroker.RabbitMq;

        /// <summary>
        /// Will trigger the add service bus method and add it to the solution. Defaulted to false to make optional for use in something like new:domain
        /// </summary>
        public bool AddBus { get; set; } = false;

        public string ProjectBaseName { get; set; }

        /// <summary>
        /// The message broker for the bus
        /// </summary>
        public string Broker
        {
            get => Enum.GetName(typeof(MessageBroker), _broker);
            set
            {
                if (!Enum.TryParse<MessageBroker>(value, true, out var parsed))
                {
                    if (value.Equals("rmq", StringComparison.InvariantCultureIgnoreCase))
                        parsed = MessageBroker.RabbitMq;
                    else
                        throw new InvalidMessageBrokerException(value);
                }
                _broker = parsed;
            }
        }

        /// <summary>
        /// List of each environment to add into the API. Optional
        /// </summary>
        public ApiEnvironment Environment { get; set; } = new ApiEnvironment();
    }
}