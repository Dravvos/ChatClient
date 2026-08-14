using ChatClient.Contracts.Conversations;
using ChatClient.Services.Api.Conversations.Interfaces;
using ChatClient.Services.Api.Http.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChatClient.Contracts.Conversations.ConversationDto;

namespace ChatClient.Services.Api.Conversations
{
    public class ConversationApiClient(IApiClient api) : IConversationApiClient
    {
        public Task<ConversationDto> CreateDirectAsync(Guid otherUserId, CancellationToken ct = default)=>
                api.PostAsync<CreateDirectConversationRequest, ConversationDto>(
                "api/conversations/direct", new CreateDirectConversationRequest(otherUserId));

        public async Task<(IReadOnlyList<MessageDto> Messages, bool HasMore)> GetMessagesAsync(Guid conversationId, DateTime? before = null, int pageSize = 30, CancellationToken ct = default)
        {
            var query = before is null ? $"pageSize={pageSize}" : $"before={before.Value:o}&pageSize={pageSize}";
            var result = await api.GetAsync<MessagesPageResponse>($"api/conversations/{conversationId}/messages?{query}", ct);

            return (result.Messages, result.HasMore);
        }
        

        public Task<IReadOnlyList<ConversationSummaryDto>> GetMyConversationsAsync(CancellationToken ct = default)=>
            api.GetAsync<IReadOnlyList<ConversationSummaryDto>>("api/conversations", ct);
    }

    public record MessagesPageResponse(IReadOnlyList<MessageDto> Messages, bool HasMore);
}
