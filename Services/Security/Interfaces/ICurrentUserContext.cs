using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Services.Security.Interfaces
{
    public interface ICurrentUserContext
    {
        Task<Guid?> GetUserIdAsync();
        Task<string> GetUserNameAsync();
        void Reset();// chamar no logout / expiração de sessão
    }
}
