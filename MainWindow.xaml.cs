using ChatClient.Contracts.Conversations;
using ChatClient.Services.Api;
using ChatClient.Services.Api.Conversations.Interfaces;
using ChatClient.Services.Api.Messages.Interfaces;
using ChatClient.Services.Realtime.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections;
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
        private readonly Func<CreateGroupWindow> _createGroupFactory;
        private readonly HubConnection _connection;

        public MainWindow(IConversationApiClient conversationApi, IChatHubClient hub, Func<CreateGroupWindow> createGroupFactory, IMessageApiClient messageApi,
            IUserApiClient userApi)
        {
            InitializeComponent();
            _conversationApi = conversationApi;
            _hub = hub;
            _createGroupFactory = createGroupFactory;
            _userApi = userApi;
            _messageApi = messageApi;
            _hub.MessageReceived += Hub_MessageRecieved;
            borderExample1.Visibility = Visibility.Collapsed;
            borderExample2.Visibility = Visibility.Collapsed;
        }

        private void Hub_MessageRecieved(object? sender, Contracts.Conversations.MessageDto e)
        {
            Dispatcher.Invoke(() =>
            {
                MessagesListBox.Items.Add($"{e.senderId}: {e.content}");
            });
        }

        private async void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            await sendMessage();
        }

        private void btnAttach_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void txtMessage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await sendMessage();
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

        private async Task sendMessage()
        {
            if (!string.IsNullOrWhiteSpace(txtMessage.Text) && _connection.State == HubConnectionState.Connected)
            {
                // Chama o método "SendMessage" definido no Hub do servidor
                await _connection.SendAsync("SendMessage", txtUserId.Text, txtMessage.Text);
                txtMessage.Clear();
                txtMessage.Focus();
            }
        }

        private async void ContactsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContactsListBox.SelectedItem != null)
            {
                var selectedConversation = (ConversationSummaryDto)ContactsListBox.SelectedItem;

                var messages = await _conversationApi.GetMessagesAsync(selectedConversation.id);

                foreach (var message in messages.Messages)
                {
                    //MessagesListBox.Items.Add($"{message.senderId}: {message.content}");
                    MessagesListBox.Items.Add(message);
                }

            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ContactsListBox.Items.Clear();
            borderMessagerRecievedExample.Visibility = Visibility.Collapsed;
            borderMessageSentExample.Visibility = Visibility.Collapsed;

            var conversations = await _conversationApi.GetMyConversationsAsync();
            foreach (var conversation in conversations)
            {
                ContactsListBox.Items.Add(conversation.id);
                /*var border = new Border();
                border = borderExample2;
                border.Visibility = Visibility.Visible;
                border.Child.*/
            }
        }

    }
}