using Microsoft.Extensions.DependencyInjection;
using RiskServerConnection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Risk
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var collection = new ServiceCollection();
            collection.AddHttpClient<UserService>(client =>
            {
                client.BaseAddress = new Uri(Constants.HttpClientURL);
                client.Timeout = TimeSpan.FromSeconds(20);
            });

            collection.AddSingleton<IGameWebSocketService, GameWebSocketService>();

            collection.AddSingleton<MainWindow>();
            collection.AddTransient<LoginWindow>();
            collection.AddTransient<RegisterWindow>();
            collection.AddTransient<IniPage>();
            collection.AddTransient<CreateGameWindow>();
            collection.AddSingleton<GameService>();



            Services = collection.BuildServiceProvider();

            
        }
    }

}
