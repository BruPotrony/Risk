using Microsoft.Extensions.DependencyInjection;
using RiskModel;
using RiskServerConnection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Lógica de interacción para GamePage.xaml
    /// </summary>
    public partial class GamePage : Page
    {
        private readonly Dictionary<Path, Brush> _originalFills = new Dictionary<Path, Brush>();

        List<Continent> continents = new List<Continent>();
        private Dictionary<long, Path> _countryPathMap = new Dictionary<long, Path>();

        public readonly GameService _gameService;

        private readonly Brush[] _availableBrushes = new[]
        {
            Brushes.IndianRed,
            Brushes.LightGreen,
            Brushes.LightGoldenrodYellow,
            Brushes.MediumPurple
        };

        private readonly Dictionary<long, Brush> _playerBrushMap = new Dictionary<long, Brush>();

        public Partida currentPartida
        {
            get { return (Partida)GetValue(currentPartidaProperty); }
            set { SetValue(currentPartidaProperty, value); }
        }

        // Using a DependencyProperty as the backing store for currentPartida.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty currentPartidaProperty =
            DependencyProperty.Register("currentPartida", typeof(Partida), typeof(GamePage), new PropertyMetadata(null));


        public GamePage()
        {
            InitializeComponent();

            _gameService = App.Services.GetRequiredService<GameService>();

        }

        private void OnPlayerLeft(long obj)
        {
            Dispatcher.Invoke(() =>
            {
                var jugador = currentPartida.Jugadors.FirstOrDefault(j => j.Id == obj);
                if (jugador != null)
                {
                    currentPartida.Jugadors.Remove(jugador);
                    refrescarListViewJugadors();
                }
            });
        }

        private void OnTornChanged(long obj)
        {
            foreach (var jugador in currentPartida.Jugadors)
            {
                jugador.isHisTurn = jugador.Id == obj;
            }

            currentPartida.TornPlayer = currentPartida.Jugadors.FirstOrDefault(j => j.Id == obj);

        }

        private void OnPlayersChanged(List<long> list)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (var id in list)
                {
                    if (currentPartida.Jugadors.All(j => j.Id != id))
                    {
                        var newPlayer = new Jugador
                        {
                            Id = id,
                            SkfUser = new Usuari
                            {
                                Id = id,
                                Username = "Player" + id.ToString(),
                                Avatar = new Avatar
                                {
                                    Id = 1,
                                    Url = "https://example.com/avatar"
                                },
                            },
                            ColorBrush = GetBrushForPlayer(id)
                        };
                        currentPartida.Jugadors.Add(newPlayer);
                    }
                }
                refrescarListViewJugadors();

                currentPartida.currentPlayers = list.Count;
                txtCurrentPlayers.Text = list.Count.ToString();

                if (currentPartida.currentPlayers == currentPartida.maxPlayers)
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                    carregarDades();
                    comencarPartida();
                }
            });
        }

        private void refrescarListViewJugadors()
        {
            lsvJugadors.ItemsSource = null;

            lsvJugadors.Items.Clear();
            foreach (var jugador in currentPartida.Jugadors)
            {
                lsvJugadors.Items.Add(jugador);
            }
        }

        private void comencarPartida()
        {
            
        }

        private void country_MouseEnter(object sender, MouseEventArgs e)
        {
            var path = (Path)sender;
            if (!_originalFills.ContainsKey(path))
                _originalFills[path] = path.Fill;
            path.Fill = new SolidColorBrush(Colors.White);
        }

        private void country_MouseLeave(object sender, MouseEventArgs e)
        {
            var path = (Path)sender;
            if (_originalFills.TryGetValue(path, out var originalBrush))
            {
                path.Fill = originalBrush;
                _originalFills.Remove(path);
            }
        }

        private async void country_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var path = (Path)sender;
            if (path.DataContext is Pais country)
            {
                await _gameService.SendOccupationAsync(country.Id,1);

                var brush = GetBrushForPlayer(currentPartida.TornPlayer.Id);

                if (!_originalFills.ContainsKey(path))
                    _originalFills[path] = path.Fill;

                path.Fill = brush;

                _originalFills[path] = brush;
            }
        }

        private async void carregarDades()
        {
            if (Application.Current.MainWindow is MainWindow mainWin)
            {
                await mainWin.RunWithLoading(async () =>
                {
                    var svc = App.Services.GetRequiredService<UserService>();
                    continents = await svc
                        .GetAllContinentsAsync()
                        .ConfigureAwait(false)
                        ?? new List<Continent>();
                });

                var allCountries = continents.SelectMany(c => c.paisos).ToList();
                var countryById = allCountries.ToDictionary(p => p.Id);

                List<Frontera> borderPairs = new List<Frontera>();
                await mainWin.RunWithLoading(async () =>
                {
                    var svc = App.Services.GetRequiredService<UserService>();
                    borderPairs = await svc.GetAllBordersAsync()
                                           .ConfigureAwait(false);
                });


                foreach (var pair in borderPairs)
                {
                    if (countryById.TryGetValue(pair.Pais1Id, out var c1) &&
                        countryById.TryGetValue(pair.Pais2Id, out var c2))
                    {
                        if (!c1.Fronteres.Contains(c2))
                            c1.Fronteres.Add(c2);
                        if (!c2.Fronteres.Contains(c1))
                            c2.Fronteres.Add(c1);
                    }
                }

                foreach (var path in MapCanvas.Children.OfType<Path>())
                {
                    path.Cursor = Cursors.Hand;

                    if (path.Tag == null)
                        continue;

                    if (long.TryParse(path.Tag.ToString(), out var id)
                        && countryById.TryGetValue(id, out var country))
                    {
                        path.DataContext = country;
                    }
                }

                _countryPathMap = MapCanvas.Children
               .OfType<Path>()
               .Where(p => p.Tag != null && long.TryParse(p.Tag.ToString(), out _))
               .GroupBy(p => long.Parse(p.Tag.ToString()))
               .ToDictionary(
                   grp => grp.Key,
                   grp => grp.First()
               );


                AfegirNumerosAlCentre();
            }
            Debug.WriteLine(continents);
        }

        private Dictionary<long, TextBlock> _countryLabelMap = new Dictionary<long, TextBlock>();

        private void AfegirNumerosAlCentre()
        {
            foreach (var kvp in _countryPathMap)
            {
                var countryId = kvp.Key;
                var path = kvp.Value;
                Rect bounds = path.RenderedGeometry.Bounds;

                double xCentro = bounds.X + bounds.Width / 2;
                double yCentro = bounds.Y + bounds.Height / 2;

                var label = new TextBlock
                {
                    Text = "0",
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    IsHitTestVisible = false,
                    Tag = countryId
                };

                label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var sizeLabel = label.DesiredSize;
                double left = xCentro - sizeLabel.Width / 2;
                double top = yCentro - sizeLabel.Height / 2;

                switch (countryId)
                {
                    case 2: top += 10; break;  
                    case 10: left -= 4; top -= 4; break; 
                    case 13: left -= 9; break; 
                    case 12: left += 9; break;
                    case 15: left += 9; break;
                    case 23: left += 10; break;
                    case 42: left += 9; break;
                    case 33: left += 9; break; 
                    case 30: top -= 33; break;
                    case 19: top -= 10; break;


                }
                Canvas.SetLeft(label, left);
                Canvas.SetTop(label, top);
                MapCanvas.Children.Add(label);

                _countryLabelMap[countryId] = label;
            }
        }


        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            currentPartida = MenuPage.currentPartida;

            currentPartida.Jugadors = new List<Jugador>();


            _gameService.PlayerListReceived += OnPlayersChanged;
            _gameService.IdTornRecived += OnTornChanged;
            _gameService.IdPlayerLeftRecived += OnPlayerLeft;
            _gameService.MapUpdatedRecived += OnMapUpdated;
            _gameService.gameStateRecived += OnGameStateChanged;

            _gameService.StartListening();


            if (!currentPartida.isCreator)
            {
                string response = await _gameService.JoinGameAsync(currentPartida.Token);
            }
            else
            {
                OnPlayersChanged(new List<long> { IniPage.currentUser.Id });
            }

            currentPartida.Okupa = new List<Okupa>();
        }

        private void OnGameStateChanged(GameState estat)
        {
            currentPartida.EstatPartida = estat;
        }

        private void OnMapUpdated(List<(long countryId, int troops, long? playerId)> list)
        {
            
            Dispatcher.Invoke(() =>
            {
                foreach (var (countryId, troops, playerId) in list)
                {
                    if (_countryPathMap.TryGetValue(countryId, out var path))
                    {
                        if (path.DataContext is Pais country)
                        {
                            var okupa = currentPartida.Okupa.FirstOrDefault(o => o.Pais.Id == countryId);
                            if (okupa == null)
                            {
                                okupa = new Okupa
                                {
                                    Pais = country,
                                    Tropes = troops,
                                    Jugador = playerId.HasValue? currentPartida.Jugadors.FirstOrDefault(j => j.Id == playerId.Value): null
                                };

                                currentPartida.Okupa.Add(okupa);
                            }
                            else
                            {
                                okupa.Tropes = troops;
                                okupa.Jugador = playerId.HasValue? currentPartida.Jugadors.FirstOrDefault(j => j.Id == playerId.Value): null;
                            }

                            if (playerId != null)
                            {
                                if (troops > 0)
                                {
                                    path.Fill = GetBrushForPlayer(long.Parse(playerId.ToString()));
                                }

                                if (_countryLabelMap.TryGetValue(countryId, out var lbl))
                                {
                                    lbl.Text = troops.ToString();
                                }
                            }
                        }
                    }
                }
            });
            
        }

        private Brush GetBrushForPlayer(long playerId)
        {
            if (!_playerBrushMap.ContainsKey(playerId))
            {
                int index = _playerBrushMap.Count % _availableBrushes.Length;
                _playerBrushMap[playerId] = _availableBrushes[index];
            }
            return _playerBrushMap[playerId];
        }

        private void btn_sortir_click(object sender, RoutedEventArgs e)
        {
            long idJugadorSortir = currentPartida.TornPlayer.Id;

            currentPartida.Jugadors.Remove(currentPartida.Jugadors.FirstOrDefault(j => j.Id == idJugadorSortir));
            _gameService.LeaveGameAsync();

            refrescarListViewJugadors();

            _gameService.StopListening();
            this.NavigationService?.Navigate(new MenuPage());


        }
    }
}
