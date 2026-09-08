using ChatClient.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Contracts.Conversations
{
    public record ConversationDto(Guid id, ConversationType type, string? name, IReadOnlyList<Guid> participantIds)
    {
        public record CreateDirectConversationRequest(Guid OtherUserId);
        public record CreateGroupConversationRequest(string Name, IReadOnlyList<Guid> ParticipantIds);
        public record AddParticipantRequest(Guid UserId);
    }
}
