using System.ComponentModel.DataAnnotations;

namespace Model.Dtos
{
    public class PaymentRequest
    {
        [Required]
        [Range(50, 1000, ErrorMessage = "Credits must be between 50 and 1000")]
        public int Credits { get; set; } // e.g. 100
    }
}
