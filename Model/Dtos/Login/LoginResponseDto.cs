using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dtos.Login
{
    public class LoginResponseDto
    {
        public string? UserName { get; set; }
        public string? AccessToken { get; set; }
        public int ExpiredIn { get; set; }
    }
}
