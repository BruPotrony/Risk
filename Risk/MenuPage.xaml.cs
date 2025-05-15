using RiskModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Risk
{
    /// <summary>
    /// Lógica de interacción para MenuPage.xaml
    /// </summary>
    public partial class MenuPage : Page
    {

        public static Partida currentPartida;

        public MenuPage()
        {
            InitializeComponent();
        }

        private void btn_create_game_click(object sender, RoutedEventArgs e)
        {
            CreateGameWindow createGameWindow = new CreateGameWindow();
            bool? dialogResult = createGameWindow.ShowDialog();

            if (dialogResult == true)
            {
                currentPartida.isCreator = true;
                this.NavigationService?.Navigate(new GamePage());
            }
        }

        private void btn_search_game_click(object sender, RoutedEventArgs e)
        {
            SearchGameWindow searchGameWindow = new SearchGameWindow();
            bool? dialogResult = searchGameWindow.ShowDialog();

            if (dialogResult == true)
            {
                currentPartida.isCreator = false;
                this.NavigationService?.Navigate(new GamePage());
            }
        }
    }
}
