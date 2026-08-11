using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Services.Api.Http
{
    public record ApiProblemDetails(
        string? type,
        string? Title,
        int? status,
        string? Detail,
        string? instance,
        IDictionary<string, string[]>? errors
        );
}
