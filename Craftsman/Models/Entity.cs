using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Craftsman.Models
{
    public class Entity
    {
        public string Name { get; set; }
        public List<EntityField> Fields { get; set; }
    }
}
