using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatClient.Services.Api.Http
{
    public class ApiException:Exception
    {
        public HttpStatusCode StatusCode { get; }
        public ApiProblemDetails? ProblemDetails { get; }
        public string? RawBody { get; }
        public ApiException(HttpStatusCode statusCode, ApiProblemDetails? problem, string? rawBody)
                    : base(problem?.Detail ?? problem?.Title ?? $"API returned {(int)statusCode} ({statusCode}).")
        {
            StatusCode = statusCode;
            ProblemDetails = problem;
            RawBody = rawBody;
        }

        public static async Task<ApiException> FromResponseAsync(
          HttpResponseMessage response, JsonSerializerOptions jsonOptions, CancellationToken ct)
        {
            var rawBody = await response.Content.ReadAsStringAsync(ct);
            ApiProblemDetails? problem = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(rawBody))
                    problem = JsonSerializer.Deserialize<ApiProblemDetails>(rawBody, jsonOptions);
            }
            catch (JsonException)
            {
                // corpo não é JSON (ex.: página de erro do IIS) — segue sem problem details
            }

            return new ApiException(response.StatusCode, problem, rawBody);
        }
    }
}
