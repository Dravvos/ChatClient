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
        private HubConnection _connection;

        public MainWindow()
        {
            InitializeComponent();
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/chatHub")
                .WithAutomaticReconnect()
                .Build();

            _connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Dispatcher.Invoke(() =>
                {
                    MessagesListBox.Items.Add($"{user}: {message}");
                    MessagesListBox.ScrollIntoView(MessagesListBox.Items[MessagesListBox.Items.Count - 1]);
                });
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
    }
}