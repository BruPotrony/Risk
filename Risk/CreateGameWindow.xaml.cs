using Microsoft.Extensions.DependencyInjection;
using RiskModel;
using RiskServerConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
    /// Lógica de interacción para CreateGameWindow.xaml
    /// </summary>
    public partial class CreateGameWindow : Window
    {

        private const int MinPlayers = 2;
        private const int MaxPlayers = 4;

        private Partida partida;

        private bool rbClicked = false;
        private IGameWebSocketService Ws => App.Services.GetRequiredService<IGameWebSocketService>();

        public CreateGameWindow()
        {
            InitializeComponent();
            partida = new Partida();

            rbClicked = false;
        }

        private void rbPublicaChecked(object sender, RoutedEventArgs e)
        {
           partida.isPublic = true;
           rbClicked = true; //Per saber si ha seleccionat si la partida es privada o publica
        }

        private void rbPrivadaChecked(object sender, RoutedEventArgs e)
        {
            partida.isPublic = false;
            rbClicked = true;
        }

        private void tbIncrease_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (int.TryParse(txtMaxPlayers.Text, out int current))
            {
                if (current < MaxPlayers)
                    txtMaxPlayers.Text = (current + 1).ToString();
            }
        }


        private void tbDecrease_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (int.TryParse(txtMaxPlayers.Text, out int current))
            {
                if (current > MinPlayers)
                    txtMaxPlayers.Text = (current - 1).ToString();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            imgAvatar.ImageSource = new BitmapImage(new Uri(IniPage.currentUser.Avatar.Url));
            txtWins.Text = IniPage.currentUser.wins.ToString();
            txtBattles.Text = IniPage.currentUser.games.ToString();
        }

        private void btn_cancel_click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private async void btn_crear_click(object sender, RoutedEventArgs e)
        {
            if (!rbClicked)
            {
                MessageBox.Show("Selecciona si la partida es privada o publica");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("El nom de la partida no pot estar buit");
                return;
            }

            partida.Nom = txtName.Text;
            partida.maxPlayers = int.Parse(txtMaxPlayers.Text);


            try
            {
                GameService gm = App.Services.GetRequiredService<GameService>();

                partida = await gm.CreateGameAsync(partida);

                MessageBox.Show("Petició de creació enviada");
                MenuPage.currentPartida = this.partida;
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar per WebSocket: {ex.Message}");
            }
        }
    }
}
