using ChatClient.Services.Api.Conversations.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for CreateGroupWindow.xaml
    /// </summary>
    public partial class CreateGroupWindow : Window
    {
        private readonly IConversationApiClient _conversationApi;

        public CreateGroupWindow(IConversationApiClient conversationApi)
        {
            InitializeComponent();
            _conversationApi = conversationApi;
        }

        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            /*
            await _conversationApi.CreateGroupAsync(txtGroupName.Text, selectedParticipantIds);
            DialogResult = true;
            Close();
            */
        }
    }
}
