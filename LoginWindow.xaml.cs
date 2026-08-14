using ChatClient.Services.Api.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using static ChatClient.Contracts.Auth.AuthDto;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly ApiClient client;
        public LoginWindow()
        {
            InitializeComponent();
            client = new ApiClient(new HttpClient { BaseAddress = new Uri("https://localhost:7163/") });
        }

        private void btnSignUp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {

            client.PostAsync<LoginRequest, AuthResponse>("api/auth/login", new LoginRequest
            (
                txtUsername.Text,
                txtPassword.Password
            )).ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    var response = task.Result;
                    // Handle successful login, e.g., open main window
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var mainWindow = new MainWindow();
                        mainWindow.Show();
                        this.Close();
                    });
                }
                else
                {
                    // Handle login failure, e.g., show error message
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Login failed: " + task.Exception?.Message);
                    });
                }
            });
        }
    }
}
