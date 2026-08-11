using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using ChatClient.Services.Security.Interfaces;
using static ChatClient.Contracts.Auth.AuthDto;

namespace ChatClient.Services.Api.Http
{
    public class AuthHeaderHandler(ITokenStore tokenStore,
        IHttpClientFactory httpClientFactory,
        IAuthSessionNotifier sessionNotifier) : DelegatingHandler
    {
        private static readonly SemaphoreSlim RefreshLock = new SemaphoreSlim(1, 1);
        private static readonly HttpRequestOptionsKey<bool> RetreidKey = new HttpRequestOptionsKey<bool>("RetriedAfterRefresh");

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (IsAuthEndpoint(request))
                return await base.SendAsync(request, cancellationToken); // login/refresh não levam Authorization

            var accessToken = await tokenStore.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken) == false)
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await base.SendAsync(request, cancellationToken);

            var alreadyRetried = request.Options.TryGetValue(RetreidKey, out var retried) && retried;

            if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized || alreadyRetried)
                return response;

            if (!await TryRefreshAsync(accessToken, cancellationToken))
                return response; // sessão realmente expirada — o 401 original vira ApiException lá na frente

            response.Dispose();

            var retryRequest = await CloneAsync(request);
            retryRequest.Options.Set(RetreidKey, true);
            retryRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await tokenStore.GetAccessTokenAsync());
            return await base.SendAsync(retryRequest, cancellationToken);
        }

        private async Task<bool> TryRefreshAsync(string? tokenUserInFailedRequest, CancellationToken cancellationToken)
        {
            await RefreshLock.WaitAsync(cancellationToken);
            try
            {
                // outra requisição concorrente pode já ter renovado enquanto esperávamos o lock —
                // nesse caso não gastamos o refresh token de novo, só usamos o access token novo

                var currentToken = await tokenStore.GetAccessTokenAsync();
                if (currentToken != tokenUserInFailedRequest)
                    return true; // outro request já fez refresh

                var refreshToken = await tokenStore.GetRefreshTokenAsync();
                if (string.IsNullOrEmpty(refreshToken))
                {
                    sessionNotifier.NotifySessionExpired();
                    return false; // sem refresh token, não dá pra renovar
                }
                var refreshApi = new ApiClient(httpClientFactory.CreateClient("AuthRefresh"));
                var result = await refreshApi.PostAsync<RefreshRequest, AuthResponse>("api/auth/refresh", new RefreshRequest(refreshToken), cancellationToken);
                await tokenStore.SaveAsync(result.AccessToken, result.RefreshToken);
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

        private static bool IsAuthEndpoint(HttpRequestMessage request) =>
       request.RequestUri?.AbsolutePath is "/api/auth/login" or "/api/auth/refresh";

        private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);
            if (request.Content is not null)
            {
                var bytes = await request.Content.ReadAsByteArrayAsync();
                clone.Content = new ByteArrayContent(bytes);
                foreach (var header in request.Content.Headers)
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            foreach (var header in request.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            return clone;
        }
    }
}