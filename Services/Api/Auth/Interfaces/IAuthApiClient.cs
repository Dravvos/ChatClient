using ChatClient.Services.Api.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Services.Api.Auth.Interfaces
{
    public interface IAuthApiClient
    {
        Task<LoginOutcome> SignUpAsync(string username, string email, string password, CancellationToken ct = default);
        Task<LoginOutcome> LoginAsync(string username, string password, CancellationToken ct = default);
        Task LogoutAsync(string refreshToken, CancellationToken ct = default);
    }
}
