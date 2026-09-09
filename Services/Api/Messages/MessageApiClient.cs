using ChatClient.Contracts.Conversations;
using ChatClient.Services.Api.Http.Interfaces;
using ChatClient.Services.Api.Messages.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Services.Api.Messages
{
    public class MessageApiClient(IApiClient api) : IMessageApiClient
    {
        public Task DeleteMessage(Guid messageId, CancellationToken ct = default) =>
            api.DeleteAsync("api/message/" + messageId, ct);

        public Task<MessageDto> SendMessageAsync(Guid conversationId, string content, CancellationToken ct = default)
        {
            var request = new SendMessageRequest(conversationId, content);
            return api.PostAsync<SendMessageRequest, MessageDto>("api/messages", request, ct);
        }

        public Task UpdateMessage(Guid messageId, string content, CancellationToken ct = default)
        {
            var request = new SendMessageRequest(Guid.Empty, content);
            return api.PutAsync<SendMessageRequest, MessageDto>($"api/messages/{messageId}", request, ct);
        }
    }

    public record SendMessageRequest(Guid ConversationId, string Content);
}
