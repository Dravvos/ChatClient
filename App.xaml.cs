using ChatClient.Services.Api;
using ChatClient.Services.Api.Auth;
using ChatClient.Services.Api.Auth.Interfaces;
using ChatClient.Services.Api.Conversations;
using ChatClient.Services.Api.Conversations.Interfaces;
using ChatClient.Services.Api.Http;
using ChatClient.Services.Api.Http.Interfaces;
using ChatClient.Services.Realtime;
using ChatClient.Services.Realtime.Interfaces;
using ChatClient.Services.Security;
using ChatClient.Services.Security.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.Net.Http;
using System.Windows;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Register your services and view models here
                    //services.Configure<ApiSettings>(configuration.GetSection("Api"));
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<ITokenStore, DpapiTokenStore>();
                    services.AddSingleton<IAuthSessionNotifier, AuthSessionNotifier>();
                    services.AddTransient<AuthHeaderHandler>();

                    services.AddHttpClient("AuthRefresh", client =>
                    {
                        client.BaseAddress = new Uri("http://localhost:5000/");
                    });

                    services.AddHttpClient("Api", client =>
                    {
                        client.BaseAddress = new Uri("http://localhost:5000/");
                    }).AddHttpMessageHandler<AuthHeaderHandler>();

                    services.AddSingleton<IApiClient>(sp=> new ApiClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api")));

                    services.AddSingleton<IAuthApiClient, AuthApiClient>();
                    services.AddSingleton<IConversationApiClient, ConversationApiClient>();
                    services.AddSingleton<ITokenRefresher, TokenRefresher>();
                    services.AddSingleton<IChatHubClient, ChatHubClient>();
                })
                .Build();
        }
    }

}
