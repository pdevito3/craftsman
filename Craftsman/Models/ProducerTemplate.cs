using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Craftsman.Models
{
    public class ProducerTemplate
    {
        public string SolutionName { get; set; }
        public List<Producer> Producers { get; set; }
    }
}