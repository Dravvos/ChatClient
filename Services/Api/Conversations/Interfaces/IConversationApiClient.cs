using ChatClient.Contracts.Conversations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Services.Api.Conversations.Interfaces
{
    public interface IConversationApiClient
    {
        Task<IReadOnlyList<ConversationSummaryDto>> GetMyConversationsAsync(CancellationToken ct = default);
        Task<ConversationDto> CreateDirectAsync(Guid otherUserId, CancellationToken ct = default);
        Task<(IReadOnlyList<MessageDto> Messages, bool HasMore)> GetMessagesAsync(
            Guid conversationId, DateTime? before = null, int pageSize = 30, CancellationToken ct = default);

    }
}
