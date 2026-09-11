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

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IConversationApiClient _conversationApi;
        private readonly IMessageApiClient _messageApi;
        private readonly IChatHubClient _hub;
        private readonly IUserApiClient _userApi;
        private readonly ICurrentUserContext _currentUserContext;
        private readonly Func<CreateGroupWindow> _createGroupFactory;
        private readonly HubConnection _connection;
        private readonly ObservableCollection<ConversationListItemViewModel> _conversations = new();
        private readonly ObservableCollection<MessageListItemViewModel> _messages = new();

        private Guid? _currentConversationId;
        private Guid? _currentUserId;

        public MainWindow(IConversationApiClient conversationApi, IChatHubClient hub, Func<CreateGroupWindow> createGroupFactory, IMessageApiClient messageApi,
            IUserApiClient userApi, ICurrentUserContext currentUserContext)
        {
            InitializeComponent();
            _conversationApi = conversationApi;
            _hub = hub;
            _createGroupFactory = createGroupFactory;
            _currentUserContext = currentUserContext;
            _userApi = userApi;
            _messageApi = messageApi;

            ContactsListBox.ItemsSource = _conversations;
            MessagesListBox.ItemsSource = _messages;

            _hub.MessageReceived += Hub_MessageRecieved;
            
        }

        private void Hub_MessageRecieved(object? sender, Contracts.Conversations.MessageDto dto)
        {
            Dispatcher.Invoke(() =>
            {
                if (dto.conversationId != _currentConversationId || _currentUserId is not { } userId)
                    return; // mensagem de uma conversa que não está aberta agora

                _messages.Add(MessageListItemViewModel.FromDto(dto, userId));
                ScrollMessagesToEnd();
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

            _currentConversationId = selected.Id;
            txtUsername.Text = selected.DisplayName;
            chatArea.Visibility = Visibility.Visible;
            await LoadMessagesAsync(selected.Id);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await _hub.StartAsync();
            _currentUserId = await _currentUserContext.GetUserIdAsync();            
            txtCurrentUsername.Text = await _currentUserContext.GetUserNameAsync();
            await LoadConversationsAsync();
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

            try
            {
                var (page, _) = await _conversationApi.GetMessagesAsync(conversationId, pageSize: 50);

                _messages.Clear();
                // servidor devolve as mais recentes primeiro; a tela precisa de ordem cronológica
                foreach (var dto in page.Reverse())
                    _messages.Add(MessageListItemViewModel.FromDto(dto, userId));

                ScrollMessagesToEnd();
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
                // Não adiciono na lista aqui: o hub devolve "MessageReceived" pro remetente
                // também (ChatService.SendMessageAsync manda pra todos os Participants), então
                // o handler acima já cobre esse caso.
            }
            catch (ChatHubException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}