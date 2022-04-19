namespace NewCraftsman.Domain
{
    using System.Collections.Generic;

    public class ProducerTemplate
    {
        public string SolutionName { get; set; }
        public List<Producer> Producers { get; set; }
    }
}