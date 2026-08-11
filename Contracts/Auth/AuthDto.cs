using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Contracts.Auth
{
    public class AuthDto
    {
        public record LoginRequest(string Username, string Password);
        public record RefreshRequest(string RefreshToken);
        public record AuthResponse(string AccessToken, string RefreshToken);
        public record AccountLockedResponse(DateTimeOffset Until);
    }
}
