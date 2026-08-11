using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Services.Api.Http.Interfaces
{
    public interface IApiClient
    {
        Task<TResponse> GetAsync<TResponse>(string requestUri, CancellationToken ct = default);
        Task<TResponse> PostAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken ct = default);
        Task PostAsync<TRequest>(string requestUri, TRequest body, CancellationToken ct = default);
        Task<TResponse> PutAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken ct = default);
        Task DeleteAsync(string requestUri, CancellationToken ct = default);
    }
}
