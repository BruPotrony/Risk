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
    /// Lógica de interacción para LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
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

            if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPwd.Password))
            {
                MessageBox.Show("Usuari o contrassenya incorrectes");
                return;
            }

            try
            {
                await RunWithLoading(async () =>
                {
                    var svc = App.Services.GetRequiredService<UserService>();
                    Usuari user = await svc.LoginAsync(txtUsername.Text, txtPwd.Password);

                    if (user != null)
                    {
                        IniPage.currentUser = user;
                    }

                    this.DialogResult = true;
                    this.Close();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en autentificar l'usuari");
            }
        }


        private void ShowLoading() => LoadingOverlay.Visibility = Visibility.Visible;
        private void HideLoading() => LoadingOverlay.Visibility = Visibility.Collapsed;
        private async Task RunWithLoading(Func<Task> work)
        {
            ShowLoading();
            try
            {
                await work();
            }
            finally
            {
                HideLoading();
            }
        }
    }
}
