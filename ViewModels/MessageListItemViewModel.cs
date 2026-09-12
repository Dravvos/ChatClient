using ChatClient.Common.Enums;
using ChatClient.Contracts.Conversations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.ViewModels
{
    public class MessageListItemViewModel : INotifyPropertyChanged
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string TimeDisplay { get;set; } = string.Empty;
        public bool IsOwnMessage { get; set; }
        
        private MessageStatus _status;

        public MessageStatus Status
        {
            get => _status;
            set
            {
                if (_status == value) return;
                _status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusIndicator));
            }
        }

        public string StatusIndicator=> Status switch
        {
            MessageStatus.Sent => "✓",
            MessageStatus.Delivered => "✓✓",
            MessageStatus.Read => "✓✓ (Read)",
            _ => string.Empty,
        };

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

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
