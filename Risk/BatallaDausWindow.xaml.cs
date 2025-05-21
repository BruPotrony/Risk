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
using System.Windows.Threading;

namespace Risk
{
    /// <summary>
    /// Lógica de interacción para BatallaDausWindow.xaml
    /// </summary>
    public partial class BatallaDausWindow : Window
    {


        public DiceAtack MyDiceAttack
        {
            get { return (DiceAtack)GetValue(MyDiceAttackProperty); }
            set { SetValue(MyDiceAttackProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyDiceAtack.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyDiceAttackProperty =
            DependencyProperty.Register("MyDiceAttack", typeof(DiceAtack), typeof(BatallaDausWindow), new PropertyMetadata(null));



        private readonly List<DispatcherTimer> _rollTimers = new();
        private readonly Random _rnd = new();

        public BatallaDausWindow(DiceAtack attackData)
        {
            InitializeComponent();
            MyDiceAttack = attackData;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowAndRoll();
        }

        public void ShowAndRoll()
        {
            StartDiceRoll();

            var stopRollTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
            stopRollTimer.Tick += (s, _) =>
            {
                stopRollTimer.Stop();
                StopDiceRoll();

                var closeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                closeTimer.Tick += (s2, _) =>
                {
                    closeTimer.Stop();
                    Close();
                };
                closeTimer.Start();
            };
            stopRollTimer.Start();
        }

        private void StartDiceRoll()
        {
            foreach (var t in _rollTimers) t.Stop();
            _rollTimers.Clear();

            foreach (var item in DiceItemsControl.Items)
            {
                var container = (ContentPresenter)DiceItemsControl
                                    .ItemContainerGenerator
                                    .ContainerFromItem(item);

                var imgAtk = FindVisualChildByName<Image>(container, "imgAttack");
                var imgDef = FindVisualChildByName<Image>(container, "imgDefend");
                if (imgAtk == null || imgDef == null) continue;

                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(80) };
                timer.Tick += (_, __) =>
                {
                    imgAtk.Source = LoadDiceImage(_rnd.Next(1, 7));
                    imgDef.Source = LoadDiceImage(_rnd.Next(1, 7));
                };
                timer.Start();
                _rollTimers.Add(timer);
            }
        }

        private void StopDiceRoll()
        {
            foreach (var t in _rollTimers) t.Stop();
            _rollTimers.Clear();

            int i = 0;
            foreach (var item in DiceItemsControl.Items)
            {
                var container = (ContentPresenter)DiceItemsControl
                                    .ItemContainerGenerator
                                    .ContainerFromItem(item);

                var imgAtk = FindVisualChildByName<Image>(container, "imgAttack");
                var imgDef = FindVisualChildByName<Image>(container, "imgDefend");
                if (imgAtk == null || imgDef == null) continue;

                var dice = MyDiceAttack.tirades[i++];
                imgAtk.Source = LoadDiceImage(dice.ResultatAtak);
                imgDef.Source = LoadDiceImage(dice.ResultatDefense);
            }
        }

        private static T? FindVisualChildByName<T>(DependencyObject parent, string name)
            where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T fe && fe.Name == name)
                    return fe;
                var result = FindVisualChildByName<T>(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        private static BitmapImage LoadDiceImage(int face)
        {
            var uri = new Uri($"/Assets/dice-{face}.png", UriKind.Relative);
            return new BitmapImage(uri);
        }
    }
}
