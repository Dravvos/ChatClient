using ChatClient.Contracts.Conversations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Services.Api.Messages.Interfaces
{
    public interface IMessageApiClient
    {
        Task DeleteMessage(Guid messageId, CancellationToken ct = default);
        Task<MessageDto> SendMessageAsync(Guid conversationId, string content, CancellationToken ct = default);
        Task UpdateMessage(Guid messageId, string content, CancellationToken ct = default);
    }
}
