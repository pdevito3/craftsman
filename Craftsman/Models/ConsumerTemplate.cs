using System.Collections.Generic;

namespace Craftsman.Models
{
    public class ConsumerTemplate
    {
        public string SolutionName { get; set; }
        public List<Consumer> Consumers { get; set; }
    }
}