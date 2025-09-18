using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entities
{
    public class HunterValidationData
    {
        public string result { get; set; } // "deliverable", "undeliverable", "risky", "unknown"
        public int score { get; set; }
        public string email { get; set; }
    }
}
