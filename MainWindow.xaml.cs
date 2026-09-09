using ChatClient.Services.Api.Conversations.Interfaces;
using ChatClient.Services.Realtime.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
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
        private readonly IChatHubClient _hub;
        private readonly Func<CreateGroupWindow> _createGroupFactory;
        private readonly HubConnection _connection;

        public MainWindow(IConversationApiClient conversationApi, IChatHubClient hub, Func<CreateGroupWindow> createGroupFactory)
        {
            InitializeComponent();
            _conversationApi = conversationApi;
            _hub = hub;
            _createGroupFactory = createGroupFactory;
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

        private void txtSearchUser_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

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

        private void ContactsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContactsListBox.SelectedItem != null)
            {
                var selectedUser = ContactsListBox.SelectedItem.ToString();
                txtUserId.Text = selectedUser;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ContactsListBox.Items.Clear();            
            var conversations = await _conversationApi.GetMyConversationsAsync();
            foreach (var conversation in conversations)
            {
                ContactsListBox.Items.Add(conversation.name);
            }
        }
    }
}