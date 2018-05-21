using System;
using System.Collections.Generic;
using System.Text;

namespace BookSpace.Factories.DTO
{
    public class UserCreateDto
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Role { get; set; }
    }
}
