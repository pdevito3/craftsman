namespace Craftsman.Domain
{
    using Enums;

    public class Consumer
    {
        public string EndpointRegistrationMethodName { get; set; }

        public string ConsumerName { get; set; }

        public string ExchangeName { get; set; }

        public string MessageName { get; set; }

        public string QueueName { get; set; }

        /// <summary>
        /// The directory we want to to put this in in the domain. Generally the plural of an entity.
        /// </summary>
        public string DomainDirectory { get; set; }

        private ExchangeTypeEnum _exchangeType { get; set; }
        public string ExchangeType
        {
            get => _exchangeType.Name;
            set
            {
                if (!ExchangeTypeEnum.TryFromName(value, true, out var parsed))
                {
                    _exchangeType = ExchangeTypeEnum.Fanout;
                    return;
                }
                
                _exchangeType = parsed;
            }
        }

        public string RoutingKey { get; set; }

        public bool IsQuorum { get; set; } = true;

        public bool IsLazy { get; set; } = true;

        public bool UsesDb { get; set; } = true;
    }
}