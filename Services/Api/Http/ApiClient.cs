using ChatClient.Services.Api.Http.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatClient.Services.Api.Http
{
    public class ApiClient(HttpClient http) : IApiClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        public async Task DeleteAsync(string requestUri, CancellationToken ct = default)
        {
            using var response = await http.DeleteAsync(requestUri, ct);
            if (!response.IsSuccessStatusCode)
            {
                throw await ApiException.FromResponseAsync(response, JsonOptions, ct);
            }
        }

        public async Task<TResponse> GetAsync<TResponse>(string requestUri, CancellationToken ct = default)
        {
            using var response = await http.GetAsync(requestUri, ct);
            if (!response.IsSuccessStatusCode)
            {
                throw await ApiException.FromResponseAsync(response, JsonOptions, ct);
            }
            var contentStream = await response.Content.ReadAsStreamAsync(ct);
            return await JsonSerializer.DeserializeAsync<TResponse>(contentStream, JsonOptions, ct)
                ?? throw new InvalidOperationException("Response content was null.");
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken ct = default)
        {
            using var response = await http.PostAsJsonAsync(requestUri, body, JsonOptions, ct);
            if (!response.IsSuccessStatusCode)
            {
                throw await ApiException.FromResponseAsync(response, JsonOptions, ct);
            }
            var contentStream = await response.Content.ReadAsStreamAsync(ct);
            return await JsonSerializer.DeserializeAsync<TResponse>(contentStream, JsonOptions, ct)
                ?? throw new InvalidOperationException("Response content was null.");
        }

        public async Task PostAsync<TRequest>(string requestUri, TRequest body, CancellationToken ct = default)
        {
            using var response = await http.PostAsJsonAsync(requestUri, body, JsonOptions, ct);
            if (!response.IsSuccessStatusCode)
            {
                throw await ApiException.FromResponseAsync(response, JsonOptions, ct);
            }
        }

        public async Task<TResponse> PutAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken ct = default)
        {
            using var response = await http.PutAsJsonAsync(requestUri, body, JsonOptions, ct);
            if (!response.IsSuccessStatusCode)
            {
                throw await ApiException.FromResponseAsync(response, JsonOptions, ct);
            }
            var contentStream = await response.Content.ReadAsStreamAsync(ct);
            return await JsonSerializer.DeserializeAsync<TResponse>(contentStream, JsonOptions, ct)
                ?? throw new InvalidOperationException("Response content was null.");
        }
    }
}
