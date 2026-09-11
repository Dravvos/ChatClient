using ChatClient.Common.Enums;
using ChatClient.Contracts.Conversations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.ViewModels
{
    public class ConversationListItemViewModel
    {
        public Guid Id { get; set; }
        public ConversationType Type { get; set; }
        public string DisplayName { get; set; } = "Conversation";
        public string LastMessagePreview { get; set; } = string.Empty;
        public string LastMessageTimeDisplay { get; set; } = string.Empty;
        public int UnreadCount { get; set; } = 0;
        public string AvatarLetter => DisplayName.Length > 0 ? DisplayName.Trim()[0].ToString().ToUpperInvariant() : "?";

        public static ConversationListItemViewModel FromDto(ConversationSummaryDto dto) => new ()
        {
            Id=dto.id,
            Type=dto.type,
            DisplayName = dto.name ?? "Conversation",
            LastMessagePreview=dto.lastMessagePreview ?? "No messages",
            LastMessageTimeDisplay= FormatTimestamp(dto.lastMessageAt),
        };

        private static string FormatTimestamp(DateTime? utc)
        {
            if (utc is not { } value) return string.Empty;

            var local = value.ToLocalTime();
            var today = DateTime.Now.Date;

            if (local.Date == today) return local.ToString("HH:mm");
            if (local.Date == today.AddDays(-1)) return "Ontem";
            if (local.Date > today.AddDays(-7)) return local.ToString("ddd", new CultureInfo("pt-BR"));
            return local.ToString("dd/MM/yyyy");
        }
    }
}
