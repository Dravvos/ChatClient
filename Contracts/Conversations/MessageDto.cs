using ChatClient.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Contracts.Conversations
{
    public record MessageDto(Guid id, Guid conversationId, Guid senderId, string senderUsername, string content, DateTime sentAt,
           DateTime? editedAt, MessageStatus status);
}
