using Microsoft.Extensions.DependencyInjection;
using RiskModel;
using RiskServerConnection;
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

namespace Risk
{
    /// <summary>
    /// Lógica de interacción para SearchGameWindow.xaml
    /// </summary>
    public partial class SearchGameWindow : Window
    {

        GameService gm;

        public SearchGameWindow()
        {
            InitializeComponent();

            gm = App.Services.GetRequiredService<GameService>();
        }



        public List<Partida> allPartides
        {
            get { return (List<Partida>)GetValue(allPartidesProperty); }
            set { SetValue(allPartidesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for allPartides.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty allPartidesProperty =
            DependencyProperty.Register("allPartides", typeof(List<Partida>), typeof(SearchGameWindow), new PropertyMetadata(null));



        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            imgAvatar.ImageSource = new BitmapImage(new Uri(IniPage.currentUser.Avatar.Url));
            txtWins.Text = IniPage.currentUser.wins.ToString();
            txtBattles.Text = IniPage.currentUser.games.ToString();

            allPartides = await gm.ListGamesAsync();


            lbPartides.ItemsSource = allPartides;
        }

        private void btn_cancel_click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private async void btn_entrar_click(object sender, RoutedEventArgs e)
        {
            var partida = lbPartides.SelectedItem as Partida;
            if (partida == null)
            {
                MessageBox.Show("Selecciona una partida");
                return;
            }

            try
            {
                MenuPage.currentPartida = partida;
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al unir a la partida: {ex.Message}");
            }
        }

        private void btn_entrar_privada_click(object sender, RoutedEventArgs e)
        {
            PartidaPrivadaTokenWindow ppt = new PartidaPrivadaTokenWindow();
            if (ppt.ShowDialog() == true)
            {
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}
