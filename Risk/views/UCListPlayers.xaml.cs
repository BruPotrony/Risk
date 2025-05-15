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

namespace Risk.views
{
    /// <summary>
    /// Lógica de interacción para UCListPlayers.xaml
    /// </summary>
    public partial class UCListPlayers : UserControl
    {


        public Jugador MyJugador
        {
            get { return (Jugador)GetValue(MyJugadorProperty); }
            set { SetValue(MyJugadorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyJugador.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyJugadorProperty =
            DependencyProperty.Register("MyJugador", typeof(Jugador), typeof(UCListPlayers), new PropertyMetadata(null));



        public UCListPlayers()
        {
            InitializeComponent();
        }
    }
}
