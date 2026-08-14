using ChatClient.Services.Api.Http;
using ChatClient.Services.Security.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static ChatClient.Contracts.Auth.AuthDto;

namespace ChatClient.Services.Security
{
    public class TokenRefresher(ITokenStore tokenStore, IHttpClientFactory httpClientFactory,
        IAuthSessionNotifier sessionNotifier) : ITokenRefresher
    {
        private static readonly SemaphoreSlim RefreshLock = new(1, 1);
        public async Task<bool> EnsureFreshTokenAsync(string? tokenSnapshot, CancellationToken cancellationToken = default)
        {
            await RefreshLock.WaitAsync(cancellationToken);
            try
            {
                var currentToken = await tokenStore.GetAccessTokenAsync();
                if (currentToken != tokenSnapshot)
                    return true; // outro request já fez refresh

                var refreshToken = await tokenStore.GetRefreshTokenAsync();
                if (string.IsNullOrEmpty(refreshToken))
                {
                    sessionNotifier.NotifySessionExpired();
                    return false;
                }

                var refreshApi = new ApiClient(httpClientFactory.CreateClient("AuthRefresh"));
                var result = await refreshApi.PostAsync<RefreshRequest, AuthResponse>("refresh", new RefreshRequest(refreshToken));
                return true;
            }
            catch (ApiException)
            {

                await tokenStore.ClearAsync();
                sessionNotifier.NotifySessionExpired();
                return false;
            }
            finally
            {
                RefreshLock.Release();
            }
        }
    }
}
