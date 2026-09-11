using ChatClient.Common.Enums;
using ChatClient.Contracts.Conversations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.ViewModels
{
    public class MessageListItemViewModel
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string TimeDisplay { get;set; } = string.Empty;
        public bool IsOwnMessage { get; set; }
        public MessageStatus Status { get; set; }

        public string StatusIndicator=> Status switch
        {
            MessageStatus.Sent => "✓",
            MessageStatus.Delivered => "✓✓",
            MessageStatus.Read => "✓✓ (Read)",
            _ => string.Empty,
        };

        public static MessageListItemViewModel FromDto(MessageDto dto, Guid currentUserId)
        {
            return new MessageListItemViewModel
            {
                Id = dto.id,
                Content = dto.content,
                TimeDisplay = dto.sentAt.ToLocalTime().ToString("HH:mm"),
                IsOwnMessage = dto.senderId == currentUserId,
                Status = dto.status
            };
        }
    }
}
