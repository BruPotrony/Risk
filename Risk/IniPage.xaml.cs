using Microsoft.Extensions.DependencyInjection;
using RiskModel;
using RiskServerConnection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Risk
{
    /// <summary>
    /// Lógica de interacción para IniPage.xaml
    /// </summary>
    public partial class IniPage : Page
    {
        private readonly UserService _userService;

        private IGameWebSocketService _ws => App.Services.GetRequiredService<IGameWebSocketService>();

        public static Usuari currentUser { get; set; }

        public IniPage()
        {
            InitializeComponent();

        }
        
        private void btn_login_click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            bool? dialogResult = loginWindow.ShowDialog();

            if (dialogResult == true && currentUser!=null)
            {
                this.NavigationService?.Navigate(new MenuPage());
                connectarWebSocket();
            }

        }

        private async void connectarWebSocket()
        {
            try
            {
                var uri = new Uri(Constants.WebSocketURL);
                await _ws.ConnectAsync(uri);

                GameService gm = App.Services.GetRequiredService<GameService>();
                await gm.RegisterAsync(currentUser.Id);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"No s'ha pogut connectar al websocket :(");
            }
        }

        private void btn_sign_up_click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            bool? dialogResult = registerWindow.ShowDialog();

            if (dialogResult == true && currentUser != null)
            {
                this.NavigationService?.Navigate(new MenuPage());
                connectarWebSocket();
            }
        }
    }
}
