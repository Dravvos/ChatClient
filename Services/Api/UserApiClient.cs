using ChatClient.Services.Api.Http.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Services.Api
{
    public class UserApiClient(IApiClient api) : IUserApiClient
    {
        public Task<Guid> GetIdByUsername(string username, CancellationToken ct = default)=>
            api.GetAsync<Guid>($"api/user/{username}", ct);
    }
}
