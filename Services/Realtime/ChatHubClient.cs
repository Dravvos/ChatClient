using ChatClient.Common.Enums;
using ChatClient.Contracts.Conversations;
using ChatClient.Services.Api;
using ChatClient.Services.Realtime.Interfaces;
using ChatClient.Services.Security.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static ChatClient.Services.Realtime.ChatHubEvent;

namespace ChatClient.Services.Realtime
{
    public class ChatHubClient : IChatHubClient
    {
        private readonly HubConnection _connection;
        private readonly ITokenStore _tokenStore;
        private readonly ITokenRefresher _tokenRefresher;
        private readonly IAuthSessionNotifier _sessionNotifier;

        public ChatConnectionState State { get; private set; } = ChatConnectionState.Disconnected;

        public event EventHandler<MessageDto>? MessageReceived;
        public event EventHandler<ChatHubEvent.MessageReadEventArgs>? MessageRead;
        public event EventHandler<ChatHubEvent.TypingEventArgs>? TypingReceived;
        public event EventHandler<ChatHubEvent.UserStatusChangedEventArgs>? UserStatusChanged;
        public event EventHandler<ChatConnectionState>? ConnectionStateChanged;

        public ChatHubClient(ITokenStore tokenStore, ITokenRefresher tokenRefresher,
            IAuthSessionNotifier sessionNotifier, IOptions<ApiSettings> apiSettings)
        {
            _tokenRefresher = tokenRefresher;
            _tokenStore = tokenStore;
            _sessionNotifier = sessionNotifier;

            _connection = new HubConnectionBuilder()
          .WithUrl($"{apiSettings.Value.BaseUrl}hubs/chat", options =>
          {
              options.AccessTokenProvider = async () => await _tokenStore.GetAccessTokenAsync();
          })
          .WithAutomaticReconnect(new[]
          {
                TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30)
          })
          .Build();

            RegisterServerEvents();
            RegisterLifecycleEvents();
        }

        private void RegisterServerEvents()
        {
            _connection.On<MessageDto>("MessageReceived", dto =>
                MessageReceived?.Invoke(this, dto));

            _connection.On<Guid, Guid, Guid>("MessageRead", (conversationId, readerUserId, messageId) =>
                MessageRead?.Invoke(this, new MessageReadEventArgs(conversationId, readerUserId, messageId)));

            _connection.On<Guid, Guid>("Typing", (conversationId, userId) =>
                TypingReceived?.Invoke(this, new TypingEventArgs(conversationId, userId)));

            _connection.On<Guid, UserStatus>("UserStatusChanged", (userId, status) =>
                UserStatusChanged?.Invoke(this, new UserStatusChangedEventArgs(userId, status)));
        }

        private void RegisterLifecycleEvents()
        {
            _connection.Reconnecting += _ =>
            {
                SetState(ChatConnectionState.Reconnecting);
                return Task.CompletedTask;
            };

            // Reconectou, mas mensagens enviadas durante a queda podem ter sido perdidas —
            // o SignalR não garante entrega nesse intervalo. Quem decide ressincronizar
            // (recarregar as últimas mensagens via REST) é a ViewModel, não este client;
            // este evento só avisa que a conexão voltou.
            _connection.Reconnected += _ =>
            {
                SetState(ChatConnectionState.Connected);
                return Task.CompletedTask;
            };

            _connection.Closed += async ex =>
            {
                SetState(ChatConnectionState.Disconnected);

                // Closed só dispara depois que o WithAutomaticReconnect já desistiu de
                // todas as tentativas. Se a causa foi token expirado, tenta renovar e
                // reconectar manualmente mais uma vez antes de desistir de vez.
                if (ex is HttpRequestException { StatusCode: HttpStatusCode.Unauthorized })
                {
                    var tokenSnapshot = await _tokenStore.GetAccessTokenAsync();
                    if (await _tokenRefresher.EnsureFreshTokenAsync(tokenSnapshot))
                        await StartAsync();
                    else
                        _sessionNotifier.NotifySessionExpired();
                }
            };
        }

        public Task MarkAsReadAsync(Guid conversationId, Guid messageId, CancellationToken ct = default)
            => InvokeAsync("MarkAsRead", conversationId, messageId, ct);

        public async Task NotifyTypingAsync(Guid conversationId, CancellationToken ct = default)
        {
            try
            {
                await _connection.SendAsync("Typing", conversationId, ct);
            }
            catch (HubException ex)
            {
                throw new ChatHubException(ex.Message);
            }
        }

        public Task SendMessageAsync(Guid conversationId, string content, CancellationToken ct = default) =>
            InvokeAsync("SendMessage", conversationId, content, ct);

        public async Task StartAsync(CancellationToken ct = default)
        {
            if (_connection.State == HubConnectionState.Disconnected)
                return;

            SetState(ChatConnectionState.Connecting);
            try
            {
                await _connection.StartAsync(ct);
                SetState(ChatConnectionState.Connected);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                var tokenSnapshot = await _tokenStore.GetAccessTokenAsync();
                if (await _tokenRefresher.EnsureFreshTokenAsync(tokenSnapshot, ct))
                {
                    await StartAsync(ct);
                    SetState(ChatConnectionState.Connected);
                }
                else
                {
                    SetState(ChatConnectionState.Disconnected);
                    _sessionNotifier.NotifySessionExpired();
                }
            }
        }

        public Task StopAsync() => _connection.StopAsync();

        private void SetState(ChatConnectionState state)
        {
            State = state;
            ConnectionStateChanged?.Invoke(this, state);
        }
        private async Task InvokeAsync(string methodName, Guid conversationId, object arg2, CancellationToken ct)
        {
            try
            {
                await _connection.InvokeAsync(methodName, conversationId, arg2, ct);
            }
            catch (HubException ex)
            {
                throw new ChatHubException(ex.Message);
            }
        }

        public async ValueTask DisposeAsync() => await _connection.DisposeAsync();
    }
}
