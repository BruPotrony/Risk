using Microsoft.Extensions.DependencyInjection;
using RiskModel;
using RiskServerConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Lógica de interacción para RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {

        private readonly List<Avatar> allAvatars = new List<Avatar>();


        private int currentIndex = 0;

        public Avatar currentAvatar
        {
            get { return (Avatar)GetValue(currentAvatarProperty); }
            set { SetValue(currentAvatarProperty, value); }
        }
        public static readonly DependencyProperty currentAvatarProperty =
            DependencyProperty.Register("currentAvatar", typeof(Avatar), typeof(RegisterWindow), new PropertyMetadata(null));



        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void btn_cancel_click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btn_before_click(object sender, RoutedEventArgs e)
        {
            if (allAvatars.Count == 0) return;

            currentIndex = (currentIndex - 1 + allAvatars.Count) % allAvatars.Count;
            currentAvatar = allAvatars[currentIndex];
        }
        private void btn_next_click(object sender, RoutedEventArgs e)
        {
            if (allAvatars.Count == 0) return;

            currentIndex = (currentIndex + 1) % allAvatars.Count;
            currentAvatar = allAvatars[currentIndex];
        }

        private async void windowReg_Loaded(object sender, RoutedEventArgs e)
        {

            try
            {
                await RunWithLoading(async () =>
                {
                    var svc = App.Services.GetRequiredService<UserService>();

                    var avatars = await svc.GetAllAvatarsAsync();

                    allAvatars.Clear();
                    allAvatars.AddRange(avatars);

                    if (allAvatars.Count > 0)
                    {
                        currentIndex = 0;
                        currentAvatar = allAvatars[0];
                    }
                    else
                    {
                        MessageBox.Show("Error en la carrega d'avatars");
                        this.DialogResult = false;
                        this.Close();
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en la carrega d'avatars");
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

        private async void btn_crear_click(object sender, RoutedEventArgs e)
        {
            const int MIN_LENGTH = 2;

            if (string.IsNullOrWhiteSpace(txbUsuari.Text) || txbUsuari.Text.Length < MIN_LENGTH ||
                string.IsNullOrWhiteSpace(txbPwd.Password) || txbPwd.Password.Length < MIN_LENGTH)
            {
                MessageBox.Show($"L'usuari i la contrassenya han de tenir com a mínim {MIN_LENGTH} caracters");
                return;
            }

            if (string.IsNullOrWhiteSpace(txbNom.Text) || txbNom.Text.Length < MIN_LENGTH ||
                string.IsNullOrWhiteSpace(txbCognom.Text) || txbCognom.Text.Length < MIN_LENGTH)
            {
                MessageBox.Show($"El nom i el cognom han de tenir com a mínim {MIN_LENGTH} lletres");
                return;
            }

            if (Regex.IsMatch(txbNom.Text, @"\d") || Regex.IsMatch(txbCognom.Text, @"\d"))
            {
                MessageBox.Show("El nom i el cognom nomes poden contenir lletres");
                return;
            }

            if (string.IsNullOrWhiteSpace(txbEmail.Text) || txbEmail.Text.Length < MIN_LENGTH)
            {
                MessageBox.Show($"El Gmail ha de tenir com a mínim {MIN_LENGTH} caracters");
                return;
            }
            if (!Regex.IsMatch(txbEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("El Gmail no es vàlid");
                return;
            }

            if (currentAvatar == null)
            {
                MessageBox.Show("Selecciona un avatar");
                return;
            }

            try
            {
                await RunWithLoading(async () =>
                {
                    var svc = App.Services.GetRequiredService<UserService>();

                    var user = new Usuari()
                    {
                        FirstName = txbNom.Text,
                        LastName = txbCognom.Text,
                        Email = txbEmail.Text,
                        Avatar = currentAvatar,
                        Username = txbUsuari.Text,
                        Password = txbPwd.Password
                    };

                    IniPage.currentUser = await svc.RegisterAsync(user);
                    this.DialogResult = true;
                    this.Close();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en la creació de l'usuari");
            }


        }
    }
}
