using ChatClient.Services.Api.Auth;
using ChatClient.Services.Api.Auth.Interfaces;
using ChatClient.Services.Api.Http;
using ChatClient.Services.Realtime.Interfaces;
using ChatClient.Services.Security.Interfaces;
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
        private readonly IAuthApiClient _authApi;
        private readonly ITokenStore _tokenStore;
        private readonly IChatHubClient _hub;
        private readonly Func<MainWindow> _mainWindowFactory;

        public LoginWindow(IAuthApiClient authApi, ITokenStore tokenStore,
            IChatHubClient hub, Func<MainWindow> mainWindowFactory)
        {
            InitializeComponent();
            _authApi = authApi;
            _tokenStore = tokenStore;
            _hub = hub;
            _mainWindowFactory = mainWindowFactory;
        }

        private async void btnSignUp_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtEmail.Text))
            {
                BorderErrorSignUp.Visibility = Visibility.Visible;
                TextErrorSignUp.Text = "Email is required";
                return;
            }
            if (string.IsNullOrEmpty(txtUsernameSignUp.Text))
            {
                BorderErrorSignUp.Visibility = Visibility.Visible;
                TextErrorSignUp.Text = "Username is required";
                return;
            }
            if (string.IsNullOrEmpty(txtPasswordSignUp.Password))
            {
                BorderErrorSignUp.Visibility = Visibility.Visible;
                TextErrorSignUp.Text = "Password is required";
                return;
            }
            BorderErrorSignUp.Visibility = Visibility.Collapsed;

            var outcome = await _authApi.SignUpAsync(txtUsernameSignUp.Text, txtEmail.Text, txtPasswordSignUp.Password);
            if (outcome is not LoginOutcome.Success s)
            {
                //mostrar o border de erro
                BorderErrorSignUp.Visibility = Visibility.Visible;
                if (outcome is LoginOutcome.InvalidCredentials i)
                    TextErrorSignUp.Text = "Invalid Credentials";
                else if (outcome is LoginOutcome.AccountLocked l)
                    TextErrorSignUp.Text = "Account locked until " + l.Until.UtcDateTime.ToLocalTime().ToString();
                else
                    TextErrorSignUp.Text = "Error while signing up: " + (outcome as LoginOutcome.ValidationFailed).Reason;
                return;
            }
            BorderErrorSignUp.Visibility = Visibility.Collapsed;
            await _tokenStore.SaveAsync(s.AccessToken, s.RefreshToken);
            await _hub.StartAsync();

            var main = _mainWindowFactory();
            Application.Current.MainWindow = main;

            main.Show();
            Close();
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(txtUsername.Text))
            {
                BorderError.Visibility = Visibility.Visible;
                TextError.Text = "Username is required";
                return;
            }

            if (string.IsNullOrEmpty(txtPassword.Password))
            {
                BorderError.Visibility = Visibility.Visible;
                TextError.Text = "Password is required";
                return;
            }

            BorderError.Visibility = Visibility.Collapsed;

            var outcome = await _authApi.LoginAsync(txtUsername.Text, txtPassword.Password);
            if (outcome is not LoginOutcome.Success s)
            {
                //mostrar o border de erro
                BorderError.Visibility = Visibility.Visible;
                if (outcome is LoginOutcome.InvalidCredentials i)
                    TextError.Text = "Invalid Credentials";
                else if (outcome is LoginOutcome.AccountLocked l)
                    TextError.Text = "Account locked until " + l.Until.UtcDateTime.ToLocalTime().ToString();
                else
                    TextError.Text = "Error while logging in: " + (outcome as LoginOutcome.ValidationFailed).Reason;
                return;
            }
            BorderError.Visibility = Visibility.Collapsed;
            await _tokenStore.SaveAsync(s.AccessToken, s.RefreshToken);
            await _hub.StartAsync();

            var main = _mainWindowFactory();
            Application.Current.MainWindow = main;

            main.Show();
            Close();
        }

        private void TextBlock_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void lblRegister_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            divSignUp.Visibility = Visibility.Visible;
            divLogin.Visibility = Visibility.Collapsed;
            BorderLogin.Visibility = Visibility.Visible;
            BorderSignup.Visibility = Visibility.Collapsed;
        }

        private void lblLogin_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            divLogin.Visibility = Visibility.Visible;
            divSignUp.Visibility = Visibility.Collapsed;
            BorderLogin.Visibility = Visibility.Collapsed;
            BorderSignup.Visibility = Visibility.Visible;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var token = await _tokenStore.GetRefreshTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                var main = _mainWindowFactory();
                Application.Current.MainWindow = main;
                main.Show();
                Close();
            }
        }
    }
}
