using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Services.Security.Interfaces
{
    public interface ITokenRefresher
    {
        Task<bool> EnsureFreshTokenAsync(string? tokenSnapshot,CancellationToken cancellationToken = default);
    }
}
