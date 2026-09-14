using ChatClient.Common.Enums;
using ChatClient.Contracts.Conversations;
using ChatClient.Services.Api;
using ChatClient.Services.Api.Conversations.Interfaces;
using ChatClient.Services.Api.Messages.Interfaces;
using ChatClient.Services.Realtime;
using ChatClient.Services.Realtime.Interfaces;
using ChatClient.Services.Security.Interfaces;
using ChatClient.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IConversationApiClient _conversationApi;
        private readonly IUserApiClient _userApi;
        private readonly ICurrentUserContext _currentUserContext;
        private readonly Func<CreateGroupWindow> _createGroupFactory;
        private readonly ObservableCollection<ConversationListItemViewModel> _conversations = new();
        private readonly ObservableCollection<MessageListItemViewModel> _messages = new();
        private readonly ChatHubClient _hub;

        private Guid? _currentConversationId;
        private Guid? _currentUserId;

        private bool _isLoadingOlderMessages;
        private bool _hasMoreOlderMessages = true;
        private ScrollViewer? _messagesScrollViewer;

        private DateTime _lastTypingNotificationSentAt = DateTime.MinValue;
        private static readonly TimeSpan TypingNotificationThrottle = TimeSpan.FromSeconds(2);

        private DispatcherTimer? _typingIndicatorHideTimer;
        private static readonly TimeSpan TypingIndicatorHideDelay = TimeSpan.FromSeconds(3);
        private string _statusTextBeforeTyping = "Online";

        public MainWindow(IConversationApiClient conversationApi, ChatHubClient hub, Func<CreateGroupWindow> createGroupFactory,
            IUserApiClient userApi, ICurrentUserContext currentUserContext)
        {
            InitializeComponent();
            _conversationApi = conversationApi;
            _hub = hub;
            _createGroupFactory = createGroupFactory;
            _currentUserContext = currentUserContext;
            _userApi = userApi;

            ContactsListBox.ItemsSource = _conversations;
            MessagesListBox.ItemsSource = _messages;

            _hub.MessageReceived += Hub_MessageRecieved;

            _hub.ConnectionStateChanged += (sender, e) =>
            {
                Console.WriteLine($"SignalR mudou ");
            };

            _hub.MessageRead += Hub_MessageRead;

            _hub._connection.Closed += async (error) =>
            {
                if (error != null)
                {
                    // ESSENCIAL: Isso vai te dizer POR QUE a conexão caiu
                    Console.WriteLine($"SignalR connection closed: {error?.Message}");
                    await Task.Delay(5000); // espera 5 segundos antes de tentar reconectar
                    try
                    {
                        await _hub.StartAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Falha ao reconectar: {ex.Message}");
                    }
                }
            };

            _hub.TypingReceived += Hub_TypingReceived;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _currentUserId = await _currentUserContext.GetUserIdAsync();
            txtCurrentUsername.Text = await _currentUserContext.GetUserNameAsync();
            await LoadConversationsAsync();
            await _hub.StartAsync();
        }

        private async void Hub_MessageRecieved(object? sender, Contracts.Conversations.MessageDto dto)
        {
            if (dto.conversationId != _currentConversationId || _currentUserId is not { } userId)
                return;

            Dispatcher.Invoke(() =>
            {
                _messages.Add(MessageListItemViewModel.FromDto(dto, userId));
                ScrollMessagesToEnd();
            });

            if (dto.senderId != userId)
            {
                try { await _hub.MarkAsReadAsync(dto.conversationId, dto.id); }
                catch (ChatHubException ex)
                {
                    Console.WriteLine($"Falha ao marcar mensagem como lida: {ex.Message}");
                    /* falha silenciosa — a próxima abertura da conversa corrige */
                }
            }
        }

        private void Hub_MessageRead(object? sender, ChatHubEvent.MessageReadEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.ConversationId != _currentConversationId)
                    return; // conversa não está aberta agora — nada pra atualizar na tela

                var readMessage = _messages.FirstOrDefault(m => m.Id == e.MessageId);

                foreach (var m in _messages)
                    if (m.IsOwnMessage && m.Status != MessageStatus.Read)
                        m.Status = MessageStatus.Read; // agora dispara PropertyChanged e atualiza o ✓✓
            });
        }

        private async void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            await SendCurrentMessageAsync();
        }

        private void btnAttach_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void txtMessage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await SendCurrentMessageAsync();
            }
        }

        private async void txtSearchUser_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var userId = await _userApi.GetIdByUsername(txtSearchUser.Text);
                await _conversationApi.CreateDirectAsync(userId);
            }
        }

        private async void ContactsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContactsListBox.SelectedItem is not ConversationListItemViewModel selected)
                return;

            _typingIndicatorHideTimer?.Stop();
            _typingIndicatorHideTimer = null;
            txtuserStatus.Text = "Online";
            txtuserStatus.Foreground = Brushes.DarkGray;

            _currentConversationId = selected.Id;
            txtUsername.Text = selected.DisplayName;
            chatArea.Visibility = Visibility.Visible;
            await LoadMessagesAsync(selected.Id);
        }

        private async Task LoadConversationsAsync()
        {
            try
            {
                var summaries = await _conversationApi.GetMyConversationsAsync();
                _conversations.Clear();
                foreach (var dto in summaries)
                    _conversations.Add(ConversationListItemViewModel.FromDto(dto));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Não foi possível carregar suas conversas: {ex.Message}");
            }
        }

        private async Task LoadMessagesAsync(Guid conversationId)
        {
            if (_currentUserId is not { } userId)
                return; // sem usuário identificado — sessão provavelmente expirada

            _hasMoreOlderMessages = true; // nova conversa, assume que pode ter histórico até provar o contrário


            try
            {
                var (page, hasMore) = await _conversationApi.GetMessagesAsync(conversationId, pageSize: 50);
                _hasMoreOlderMessages = hasMore;
                _messages.Clear();
                // servidor devolve as mais recentes primeiro; a tela precisa de ordem cronológica
                foreach (var dto in page.Reverse())
                {
                    _messages.Add(MessageListItemViewModel.FromDto(dto, userId));
                }

                ScrollMessagesToEnd();

                var lastFromOther = page.FirstOrDefault(m => m.senderId != userId); // page vem mais recentes primeiro
                if (lastFromOther is not null)
                    await _hub.MarkAsReadAsync(conversationId, lastFromOther.id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Não foi possível carregar as mensagens: {ex.Message}");
            }
        }

        private void ScrollMessagesToEnd()
        {
            if (_messages.Count > 0)
                MessagesListBox.ScrollIntoView(_messages[^1]);
        }

        private async Task SendCurrentMessageAsync()
        {
            if (_currentConversationId is not { } conversationId || string.IsNullOrWhiteSpace(txtMessage.Text))
                return;

            try
            {
                await _hub.SendMessageAsync(conversationId, txtMessage.Text.Trim());
                txtMessage.Clear();
                txtMessage.Focus();
            }
            catch (ChatHubException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private ScrollViewer? GetMessagesScrollViewer()
        {
            if (_messagesScrollViewer is not null) return _messagesScrollViewer;
            _messagesScrollViewer = FindScrollViewer(MessagesListBox);
            return _messagesScrollViewer;


        }
        private ScrollViewer? FindScrollViewer(DependencyObject o)
        {
            if (o is ScrollViewer sv) return sv;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var found = FindScrollViewer(VisualTreeHelper.GetChild(o, i));
                if (found is not null) return found;
            }
            return null;
        }
        private void MessagesListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentWidthChange != 0) return;

            if (e.VerticalOffset < 40 && e.VerticalChange < 0)
                _ = TryLoadOlderMessagesAsync();
        }

        private async Task TryLoadOlderMessagesAsync()
        {
            if (_isLoadingOlderMessages || !_hasMoreOlderMessages) return;
            if (_currentConversationId is not { } conversationId || _currentUserId is not { } userId) return;
            if (_messages.Count == 0) return;

            _isLoadingOlderMessages = true;
            LoadingOlderIndicator.Visibility = Visibility.Visible;

            var scrollViewer = GetMessagesScrollViewer();
            var previousExtentHeight = scrollViewer?.ExtentHeight ?? 0;
            var previousOffset = scrollViewer?.VerticalOffset ?? 0;

            try
            {
                // _messages fica sempre em ordem cronológica ([0] = mais antiga carregada),
                // então o cursor pra "antes disso" é o SentAt do primeiro item.
                var oldestSentAt = _messages[0].SentAt;
                var (page, hasMore) = await _conversationApi.GetMessagesAsync(conversationId, before: oldestSentAt, pageSize: 50);

                if (conversationId != _currentConversationId)
                    return; // usuário trocou de conversa enquanto a página antiga ainda carregava

                _hasMoreOlderMessages = hasMore;
                if (page.Count == 0) return;

                // servidor devolve mais recentes primeiro; insere na ordem inversa pra manter cronologia no topo
                var olderItems = page.Reverse().Select(dto => MessageListItemViewModel.FromDto(dto, userId)).ToList();
                for (int i = 0; i < olderItems.Count; i++)
                    _messages.Insert(i, olderItems[i]);

                if (scrollViewer is not null)
                {
                    // espera o layout medir a nova altura do conteúdo antes de reposicionar o scroll
                    await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Loaded);
                    var newExtentHeight = scrollViewer.ExtentHeight;
                    scrollViewer.ScrollToVerticalOffset(previousOffset + (newExtentHeight - previousExtentHeight));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Não foi possível carregar mensagens antigas: {ex.Message}");
            }
            finally
            {
                _isLoadingOlderMessages = false;
                LoadingOlderIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private async void txtMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_currentConversationId is not { } conversationId) return;
            if (string.IsNullOrEmpty(txtMessage.Text)) return; // campo limpo (ex.: após enviar) não conta como "digitando"

            var now = DateTime.UtcNow;
            if (now - _lastTypingNotificationSentAt < TypingNotificationThrottle) return;
            _lastTypingNotificationSentAt = now;

            try
            {
                await _hub.NotifyTypingAsync(conversationId);
            }
            catch (Exception)
            {
                // best-effort — indicador de "digitando" não é crítico, uma falha aqui não deve incomodar o usuário
            }
        }

        private void Hub_TypingReceived(object? sender, ChatHubEvent.TypingEventArgs e)
        {
            if (e.ConversationId != _currentConversationId) return;
            Dispatcher.Invoke(() =>
            {
                
            });
        }

        private void ShowTypingIndicator()
        {
            if(_typingIndicatorHideTimer is null)
                _statusTextBeforeTyping = txtuserStatus.Text;

            txtuserStatus.Text = "typing...";

            txtuserStatus.Foreground = new SolidColorBrush(Color.FromRgb(0x6d, 0xa7, 0xec));

            _typingIndicatorHideTimer?.Stop();
            _typingIndicatorHideTimer = new DispatcherTimer { Interval = TypingIndicatorHideDelay };
            _typingIndicatorHideTimer.Tick += TypingIndicatorHideTimer_Tick;
            _typingIndicatorHideTimer.Start();
        }
        private void TypingIndicatorHideTimer_Tick(object? sender, EventArgs e)
        {
            _typingIndicatorHideTimer?.Stop();
            _typingIndicatorHideTimer = null;
            txtuserStatus.Text = _statusTextBeforeTyping;
            txtuserStatus.Foreground = Brushes.DarkGray;
        }

    }
}