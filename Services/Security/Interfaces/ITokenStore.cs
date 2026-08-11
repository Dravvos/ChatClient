using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Services.Security.Interfaces
{
    public interface ITokenStore
    {
        Task<string?> GetAccessTokenAsync();
        Task<string?> GetRefreshTokenAsync();
        Task SaveAsync(string accessToken, string refreshToken, bool persistRefreshToken = true);
        Task ClearAsync();
    }
}
