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
    /// Lógica de interacción para UCListGames.xaml
    /// </summary>
    public partial class UCListGames : UserControl
    {



        private bool isClicked = false;


        public Partida MyPartida
        {
            get { return (Partida)GetValue(MyPartidaProperty); }
            set { SetValue(MyPartidaProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyPartida.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyPartidaProperty =
            DependencyProperty.Register("MyPartida", typeof(Partida), typeof(UCListGames), new PropertyMetadata(null));



        public UCListGames()
        {
            InitializeComponent();
        }

        

        
    }
}
