namespace Craftsman.Domain
{
    using System.Collections.Generic;

    public class ConsumerTemplate
    {
        public string SolutionName { get; set; }
        public List<Consumer> Consumers { get; set; }
    }
}