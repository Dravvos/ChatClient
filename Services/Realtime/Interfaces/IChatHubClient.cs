using ChatClient.Common.Enums;
using ChatClient.Contracts.Conversations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChatClient.Services.Realtime.ChatHubEvent;

namespace ChatClient.Services.Realtime.Interfaces
{
    public interface IChatHubClient:IAsyncDisposable
    {
        ChatConnectionState State { get; }

        event EventHandler<MessageDto>? MessageReceived;
        event EventHandler<MessageReadEventArgs>? MessageRead;
        event EventHandler<TypingEventArgs>? TypingReceived;
        event EventHandler<UserStatusChangedEventArgs>? UserStatusChanged;
        event EventHandler<ChatConnectionState>? ConnectionStateChanged;

        Task StartAsync(CancellationToken ct = default);
        Task StopAsync();

        Task SendMessageAsync(Guid conversationId, string content, CancellationToken ct = default);
        Task MarkAsReadAsync(Guid conversationId, Guid messageId, CancellationToken ct = default);
        Task NotifyTypingAsync(Guid conversationId, CancellationToken ct = default);

    }
}
