using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dtos.Response
{
    public class WorkflowResponse
    {
        public bool Success { get; set; }
        public string? ImageData { get; set; } // Base64 encoded image
        public string? ImageFormat { get; set; } // "png", "jpeg"
        public string? Error { get; set; }
        public int StatusCode { get; set; }
        public string? RawResponse { get; set; } // For debugging
    }
}
