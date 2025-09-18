using System.ComponentModel.DataAnnotations;

namespace Model.Dtos.Request
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
