using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Contracts.Conversations
{
    public record ConversationSummaryDto(Guid id, ConversationType type, string? name, string? lastMessagePreview, DateTime? lastMessageAt, int unreadCount);
}
