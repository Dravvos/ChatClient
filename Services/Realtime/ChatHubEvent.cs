using ChatClient.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Services.Realtime
{
    public class ChatHubEvent
    {
        public record MessageReadEventArgs(Guid ConversationId, Guid ReaderUserId, Guid MessageId);
        public record TypingEventArgs(Guid ConversationId, Guid UserId);
        public record UserStatusChangedEventArgs(Guid UserId, UserStatus Status);
    }
}
