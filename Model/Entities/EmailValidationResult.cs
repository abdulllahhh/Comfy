using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entities
{
    public class EmailValidationResult
    {
        public string Email { get; set; }
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
