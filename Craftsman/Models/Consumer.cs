namespace Craftsman.Models
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using System;

    public class Consumer
    {
        private ExchangeType _type = Enums.ExchangeType.Direct;

        public string EndpointRegistrationMethodName { get; set; }

        public string ConsumerName { get; set; }

        public string ExchangeName { get; set; }

        public string MessageName { get; set; }

        public string QueueName { get; set; }

        public string ExchangeType
        {
            get => Enum.GetName(typeof(ExchangeType), _type);
            set
            {
                if (!Enum.TryParse<ExchangeType>(value, true, out var parsed))
                {
                    throw new InvalidExchangeTypeException(value);
                }
                _type = parsed;
            }
        }

        public string RoutingKey { get; set; }

        public bool IsQuorum { get; set; } = true;

        public bool IsLazy { get; set; } = true;

        public bool UsesDb { get; set; }
    }
}