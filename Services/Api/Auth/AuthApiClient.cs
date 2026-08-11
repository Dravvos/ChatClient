using ChatClient.Services.Api.Auth.Interfaces;
using ChatClient.Services.Api.Http;
using ChatClient.Services.Api.Http.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static ChatClient.Contracts.Auth.AuthDto;

namespace ChatClient.Services.Api.Auth
{
    public class AuthApiClient(IApiClient api) : IAuthApiClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public async Task<LoginOutcome> LoginAsync(string username, string password, CancellationToken ct = default)
        {
            try
            {
                var response = await api.PostAsync<LoginRequest, AuthResponse>("api/auth/login", new LoginRequest(username, password), ct);
                return new LoginOutcome.Success
                (
                    response.AccessToken,
                    response.RefreshToken
                );
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return new LoginOutcome.InvalidCredentials();
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Locked)
            {
                var locked = JsonSerializer.Deserialize<AccountLockedResponse>(ex.RawBody ?? "{}", JsonOptions);
                return new LoginOutcome.AccountLocked
                (
                    locked?.Until ?? DateTimeOffset.UtcNow.AddMinutes(15)
                );
            }
        }

        public Task LogoutAsync(string refreshToken, CancellationToken ct = default)=>
            api.PostAsync("api/auth/logout", new RefreshRequest(refreshToken), ct);
    }
}
