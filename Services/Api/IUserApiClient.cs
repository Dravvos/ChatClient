using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Services.Api
{
    public interface IUserApiClient
    {
        Task<Guid> GetIdByUsername(string username, CancellationToken ct = default);
    }
}
