using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Services.Api.Auth
{
    public abstract record LoginOutcome
    {
        public sealed record Success(string AccessToken, string RefreshToken) : LoginOutcome;
        public sealed record InvalidCredentials : LoginOutcome;
        public sealed record AccountLocked(DateTimeOffset Until) : LoginOutcome;
    }
}
