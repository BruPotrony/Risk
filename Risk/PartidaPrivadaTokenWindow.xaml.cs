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
using System.Windows.Shapes;

namespace Risk
{
    /// <summary>
    /// Lógica de interacción para PartidaPrivadaTokenWindow.xaml
    /// </summary>
    public partial class PartidaPrivadaTokenWindow : Window
    {
        public PartidaPrivadaTokenWindow()
        {
            InitializeComponent();
        }


        private void btn_cancel_click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private async void btn_save_click(object sender, RoutedEventArgs e)
        {
            Partida p = new Partida();
            p.Token = txtToken.Text;
            p.isPublic = false;

            if (string.IsNullOrEmpty(p.Token))
            {
                MessageBox.Show("El token no pot estar buit.");
                return;
            }

            MenuPage.currentPartida = p;

            this.DialogResult = true;
            this.Close();
        }
    }
}
