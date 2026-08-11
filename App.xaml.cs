using System.Configuration;
using System.Data;
using System.Windows;
using ChatClient.Services.Security;
using ChatClient.Services.Security.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<ITokenStore, DpapiTokenStore>();
                })
                .Build();
        }
    }

}
