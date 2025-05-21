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
    /// Lógica de interacción para ContadorTropesAtac.xaml
    /// </summary>
    public partial class ContadorTropesAtac : Window
    {


        public int troopsCount
        {
            get { return (int)GetValue(troopsCountProperty); }
            set { SetValue(troopsCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for troopsCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty troopsCountProperty =
            DependencyProperty.Register("troopsCount", typeof(int), typeof(ContadorTropesAtac), new PropertyMetadata(1));



        public Pais MyPais
        {
            get { return (Pais)GetValue(MyPaisProperty); }
            set { SetValue(MyPaisProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyPais.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyPaisProperty =
            DependencyProperty.Register("MyPais", typeof(Pais), typeof(ContadorTropesAtac), new PropertyMetadata(null));





        public ContadorTropesAtac(Pais p, string title)
        {
            InitializeComponent();
            MyPais = p;

            txtTittle.Text = title;
        }

        private void btn_cancel_click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btn_increase_click(object sender, RoutedEventArgs e)
        {
            if ( troopsCount < MyPais.Tropes-1 )
            {
                troopsCount++;
            }
        }

        private void btn_decrease_click(object sender, RoutedEventArgs e)
        {
            if (troopsCount > 1)
            {
                troopsCount--;
            }
        }

        private void btn_attack_click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
