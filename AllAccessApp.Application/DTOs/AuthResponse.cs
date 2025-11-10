using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllAccessApp.Application.DTOs
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; } 
        public string? Token { get; set; }
        public int UserId { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }

    }
}
