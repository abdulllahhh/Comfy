using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dtos.Request
{
    public class WorkflowRequest
    {
        [Required]
        [StringLength(1000, ErrorMessage = "Input too long")]
        public string Prompt { get; set; }
        public int Seed { get; set; } = 1234;
        public int Steps { get; set; } = 20;
        public int Cfg { get; set; } = 8;
        //public string FilenamePrefix { get; set; } = "run001";
    }
}
