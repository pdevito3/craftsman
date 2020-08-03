using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Craftsman.Models
{
    public class EntityProperty
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool CanFilter { get; set; } = false;
        public bool CanSort { get; set; } = false;
        public bool CanManipulate { get; set; } = true;
        public bool IsPrimaryKey { get; set; } = false;
        public bool IsRequired { get; set; } = false;
    }
}
