using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Contracts.Conversations
{
    public class ConversationDto
    {
        public record CreateDirectConversationRequest(Guid OtherUserId);
        public record CreateGroupConversationRequest(string Name, IReadOnlyList<Guid> ParticipantIds);
        public record AddParticipantRequest(Guid UserId);
    }
}
