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

        private int troopsToPlace;

        private readonly Brush[] _availableBrushes = new[]
        {
            Brushes.IndianRed,
            Brushes.LightGreen,
            Brushes.LightGoldenrodYellow,
            Brushes.MediumPurple
        };

        public Pais? fromCountry = null;
        public Pais? toCountry = null;

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



        private void OnPlayersChanged(List<PlayerAux> list)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (var p in list)
                {
                    if (currentPartida.Jugadors.All(j => j.Id != p.Id))
                    {
                        var newPlayer = new Jugador
                        {
                            Id = p.Id,
                            SkfUser = new Usuari
                            {
                                Id = p.Id,
                                Username = p.Username,
                                Avatar = new Avatar
                                {
                                    Url = p.AvatarUrl
                                }
                            },
                            SkfPartida = currentPartida,
                            ColorBrush = GetBrushForPlayer(p.Id)
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

                    if (_countryPathMap != null || _countryPathMap.Count == 0)
                    {
                        carregarDades();
                    }
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

        private readonly Dictionary<Path, Brush> _originalStrokes = new();
        private readonly Dictionary<Path, double> _originalStrokeThickness = new();

        private void RestoreAllBorders()
        {
            foreach (var kv in _originalStrokes)
            {
                var path = kv.Key;
                path.Stroke = kv.Value;
                path.StrokeThickness = _originalStrokeThickness[path];
            }

            _originalStrokes.Clear();
            _originalStrokeThickness.Clear();
        }


        private async void country_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (currentPartida.TornPlayer.SkfUser.Id != IniPage.currentUser.Id)
            {
                MessageBox.Show("Espera al teu torn!");
                return;
            }

            var path = (Path)sender;
            if (path.DataContext is Pais country)
            {
                var allCountries = continents.SelectMany(c => c.paisos).ToList();

                if (currentPartida.EstatPartida == GameState.NotStarted)
                {
                    await llogicaJocNoComencat(path, country, allCountries);

                }
                else if (currentPartida.EstatPartida == GameState.Attaking)
                {
                    await llogicaAtac(country);
                }
                else if (currentPartida.EstatPartida == GameState.Reforce)
                {
                    await llogicaReforc(path, country);
                }
                else if (currentPartida.EstatPartida == GameState.Bonus)
                {
                    if (country.PaisDeJugador != null && country.PaisDeJugador.Id == currentPartida.TornPlayer.Id)
                    {
                        if (troopsToPlace > 0)
                        {
                            int troops = getBonusTroops(troopsToPlace, "Tropes a posar");

                            if (troops <= 0 || troops > troopsToPlace)
                            {
                                return;
                            }
                            await _gameService.SendBonusAsync(country.Id, troops);
                        }
                    }
                }


            }
        }

        private async Task llogicaReforc(Path path, Pais country)
        {
            if (country.PaisDeJugador != null && country.PaisDeJugador.Id == currentPartida.TornPlayer.Id)
            {
                if (fromCountry == null)
                {
                    getPaisFrom(country);
                    pintarBorder(path, Brushes.White);
                }
                else if (fromCountry != null && fromCountry != country)
                {
                    int tropes = getTroops(fromCountry, "Tropes per reforçar");
                    if (tropes <= 0 || tropes >= fromCountry.Tropes)
                    {
                        return;
                    }

                    toCountry = country;
                    await _gameService.SendFortifyAsync(fromCountry.Id, toCountry.Id, tropes);

                    fromCountry = null;
                    toCountry = null;
                    RestoreAllBorders();
                }
            }
            else
            {
                MessageBox.Show("Has de seleccionar només països teus!");
            }
        }

        private void pintarBorder(Path path, Brush borderColor)
        {
            if (!_originalStrokes.ContainsKey(path))
            {
                _originalStrokes[path] = path.Stroke;
                _originalStrokeThickness[path] = path.StrokeThickness;
            }

            path.Stroke = borderColor;
            path.StrokeThickness = 4;
        }


        private async Task llogicaAtac(Pais country)
        {
            if (fromCountry == null)
            {
                getPaisFrom(country);
                if (fromCountry == null) return;

                HighlightAttackableNeighbors(fromCountry);
                return;
            }
            else
            {
                bool esPaisVei = fromCountry.Fronteres.Any(vei => vei.Id == country.Id);

                if (country.PaisDeJugador.Id == currentPartida.TornPlayer.Id)
                {
                    fromCountry = null;
                    getPaisFrom(country);
                    if (fromCountry == null) return;

                    HighlightAttackableNeighbors(fromCountry);
                    return;
                }
                else
                {
                    if (esPaisVei)
                    {
                        int tropesAtacants = getTroops(fromCountry, "Tropes Atacants");

                        if (tropesAtacants <= 0 || tropesAtacants >= fromCountry.Tropes)
                        {
                            return;
                        }

                        toCountry = country;
                        await _gameService.SendAttackAsync(fromCountry.Id, toCountry.Id, tropesAtacants);
                    }
                    else
                    {
                        MessageBox.Show("Has d'atacar un pais veí!");
                    }
                }
            }
        }

        private async Task llogicaJocNoComencat(Path path, Pais country, List<Pais> allCountries)
        {
            bool hiHaSenseOcupar = allCountries.Any(p => p.PaisDeJugador == null);
            if (hiHaSenseOcupar && country.PaisDeJugador != null && country.PaisDeJugador.Id == currentPartida.TornPlayer.Id)
            {
                MessageBox.Show("Has d'ocupar tots els països abans de començar la reforçar!");
                return;
            }

            if (country.PaisDeJugador != null && currentPartida.TornPlayer.Id != country.PaisDeJugador.Id)
            {
                MessageBox.Show("Has d'ocupar un Pais teu!");
                return;
            }

            await _gameService.SendOccupationAsync(country.Id, 1);
            country.Tropes += 1;
            country.PaisDeJugador = currentPartida.TornPlayer;
            pintarPais(path);
        }

        private int getTroops(Pais country, string tittle)
        {
            ContadorTropesAtac contador = new ContadorTropesAtac(country, tittle);
            if (contador.ShowDialog() == true)
            {
                int tropes = contador.troopsCount;

                return tropes;
            }
            else
            {
                return -1;
            }
        }

        private int getBonusTroops(int maxTroops, string tittle)
        {
            ContadorTropesBonus contador = new ContadorTropesBonus(maxTroops, tittle);
            if (contador.ShowDialog() == true)
            {
                int tropes = contador.troopsCount;

                return tropes;
            }
            else
            {
                return -1;
            }
        }

        private void pintarPais(Path path)
        {
            var brush = GetBrushForPlayer(currentPartida.TornPlayer.Id);

            if (!_originalFills.ContainsKey(path))
                _originalFills[path] = path.Fill;

            path.Fill = brush;

            _originalFills[path] = brush;
        }

        private void HighlightAttackableNeighbors(Pais country)
        {
            RestoreAllBorders();

            var myId = currentPartida.TornPlayer.Id;
            foreach (var vei in country.Fronteres)
            {
                if (vei.PaisDeJugador?.Id != myId
                    && _countryPathMap.TryGetValue(vei.Id, out var veiPath))
                {
                    pintarBorder(veiPath, Brushes.White);
                }
            }
        }





        private void getPaisFrom(Pais country)
        {
            if (fromCountry != null)
                return;

            if (country.PaisDeJugador != null && currentPartida.TornPlayer.Id != country.PaisDeJugador.Id)
            {
                MessageBox.Show("Has de seleccionar un pais teu!");
                return;
            }
            if (country.Tropes == 1)
            {
                MessageBox.Show("Has de seleccionar un pais amb mes d'una tropa!");
                return;
            }
            if (fromCountry == null)
            {
                fromCountry = country;
            }
        }

        private async Task carregarDades()
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

                FillCountryPathMap();


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

                if (!(path.DataContext is Pais country))
                    continue;
                string texto = country.Tropes.ToString();

                Rect bounds = path.RenderedGeometry.Bounds;
                double xCentro = bounds.X + bounds.Width / 2;
                double yCentro = bounds.Y + bounds.Height / 2;

                double left = xCentro;
                double top = yCentro;
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

                if (_countryLabelMap.TryGetValue(countryId, out var existingLabel))
                {
                    existingLabel.Text = texto;
                    existingLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    var size = existingLabel.DesiredSize;
                    Canvas.SetLeft(existingLabel, left - size.Width / 2);
                    Canvas.SetTop(existingLabel, top - size.Height / 2);
                }
                else
                {
                    var label = new TextBlock
                    {
                        Text = texto,
                        FontSize = 12,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.Black,
                        IsHitTestVisible = false,
                        Tag = countryId
                    };

                    label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    var sizeLabel = label.DesiredSize;

                    Canvas.SetLeft(label, left - sizeLabel.Width / 2);
                    Canvas.SetTop(label, top - sizeLabel.Height / 2);
                    MapCanvas.Children.Add(label);

                    _countryLabelMap[countryId] = label;
                }
            }
        }



        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            currentPartida = MenuPage.currentPartida;

            currentPartida.Jugadors = new List<Jugador>();

            currentPartida.Okupa = new List<Okupa>();




            _gameService.PlayerListReceived += OnPlayersChanged;
            _gameService.IdTornRecived += OnTornChanged;
            _gameService.IdPlayerLeftRecived += OnPlayerLeft;
            _gameService.MapUpdatedRecived += OnMapUpdated;
            _gameService.gameStateRecived += OnGameStateChanged;
            _gameService.DiceAttackRecived += OnDiceAttackChanged;
            _gameService.attackInitializedRecived += OnAttackInitialized;
            _gameService.territoryConqueredRecived += OnTerritoryConqueredRecived;
            _gameService.territoryUnderAttackRecived += OnTerritoryConqueredRecived;
            _gameService.totalTroopsToPlaceRecived += OnTotalTroopsToPlaceRecived;
            _gameService.winRecived += OnWinRecived;
            _gameService.joinedGameRecived += OnJoinedGameRecived;
            _gameService.gameStartedRecived += OnGameStart;

            _gameService.StartListening();


            if (!currentPartida.isCreator)
            {

                await _gameService.JoinGameAsync(currentPartida.Token);

                
                

                Debug.WriteLine("Partida unida: " + currentPartida.Nom);

            }
            else
            {
                OnPlayersChanged(new List<PlayerAux> {
                                    new (
                                        IniPage.currentUser.Avatar.Url,
                                        IniPage.currentUser.Id,
                                        IniPage.currentUser.Username
                                    )
                                });

            }


            if (!currentPartida.isPublic)
            {
                txbInformatiu.Text = "CODI: "+ currentPartida.Token;
                Debug.WriteLine("Codi de la partida: " + currentPartida.Token);
            }


        }

        private void OnGameStart()
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;

            if (_countryPathMap != null || _countryPathMap.Count == 0)
            {
                carregarDades();
            }
        }

        private void OnJoinedGameRecived(Partida partida)
        {
            if (!currentPartida.isPublic)
            {
                currentPartida.Nom = partida.Nom;
                currentPartida.maxPlayers = partida.maxPlayers;
                currentPartida.Token = partida.Token;
            }
            

        }

        private async void OnWinRecived()
        {
            
            txbMessageOverlay.Text = "You Win!!!";
            MessageOverlay.Visibility = Visibility.Visible;

            await Task.Delay(TimeSpan.FromSeconds(3));

            MessageOverlay.Visibility = Visibility.Collapsed;

            _gameService.LeaveGameAsync();

            this.NavigationService?.Navigate(new MenuPage());

            


        }

        private void OnTotalTroopsToPlaceRecived(long troops)
        {
            troopsToPlace = (int)troops;

            txbInformatiu.Text = "Tens " + troopsToPlace + " tropes a col·locar";

        }

        private void OnTerritoryConqueredRecived(long fromCountry, long toCountry)
        {
            pintarBorder(_countryPathMap[toCountry], Brushes.Red);
            pintarBorder(_countryPathMap[fromCountry], Brushes.White);
        }

        private async void OnTerritoryConqueredRecived()
        {
            if (fromCountry != null && fromCountry.PaisDeJugador.Id == currentPartida.TornPlayer.Id)
            {
                int tropesMoure;
                do
                {
                    tropesMoure = getTroops(fromCountry, "Tropes a moure");
                    if (tropesMoure == -1)
                    {
                        MessageBox.Show("Has de seleccionar les tropes a moure");
                    }
                }
                while (tropesMoure == -1);

                await _gameService.SendMoveTroopsAsync(tropesMoure);
            }
        }


        private void OnAttackInitialized(long obj)
        {
            toCountry = continents
                        .SelectMany(c => c.paisos)
                        .FirstOrDefault(p => p.Id == obj);
            Debug.WriteLine($"Pais defensor: {toCountry?.Nom} ({toCountry?.Id})");
        }

        private void OnDiceAttackChanged(List<int> attackerDice, List<int> defenderDice)
        {
            var paired = new List<Dice>();
            int max = Math.Max(attackerDice.Count, defenderDice.Count);

            for (int i = 0; i < max; i++)
            {
                paired.Add(new Dice
                {
                    ResultatAtak = i < attackerDice.Count ? attackerDice[i] : 0,
                    ResultatDefense = i < defenderDice.Count ? defenderDice[i] : 0
                });
            }

            var diceAttack = new DiceAtack
            {
                Atacant = currentPartida.TornPlayer,
                Defensor = toCountry.PaisDeJugador,
                tirades = paired
            };

            Debug.WriteLine(diceAttack);

            var window = new BatallaDausWindow(diceAttack);
            window.ShowDialog();


            RestoreAllBorders();
        }



        private async void OnGameStateChanged(GameState estat)
        {
            currentPartida.EstatPartida = estat;
            RestoreAllBorders();
            fromCountry = null;
            toCountry = null;

            if (currentPartida.TornPlayer.SkfUser.Id == IniPage.currentUser.Id && GameState.Attaking == currentPartida.EstatPartida)
            {
                btnSegTorn.Visibility = Visibility.Visible;

            }
            else if (currentPartida.TornPlayer.SkfUser.Id == IniPage.currentUser.Id && GameState.Reforce == currentPartida.EstatPartida)
            {
                btnSegTorn.Visibility = Visibility.Visible;
            }
            else
            {
                btnSegTorn.Visibility = Visibility.Hidden;
            }


            switch (estat)
            {
                case GameState.Attaking:

                    btnSegTorn.Content = "Seguent Torn";

                    txbInformatiu.Text = "Ataca un pais veí!";

                    txbMessageOverlay.Text = "Fase d'atac";
                    MessageOverlay.Visibility = Visibility.Visible;

                    await Task.Delay(TimeSpan.FromSeconds(3));

                    MessageOverlay.Visibility = Visibility.Collapsed;
                    break;

                case GameState.Reforce:

                    btnSegTorn.Content = "Finalitzar Torn";

                    txbInformatiu.Text = "Reforça els països!";

                    txbMessageOverlay.Text = "Fase de reforç";
                    MessageOverlay.Visibility = Visibility.Visible;

                    await Task.Delay(TimeSpan.FromSeconds(3));

                    MessageOverlay.Visibility = Visibility.Collapsed;
                    break;

                case GameState.Bonus:

                    txbInformatiu.Text ="Tens "+ troopsToPlace+" tropes a col·locar";

                    txbMessageOverlay.Text = "Fase de Bonus";
                    MessageOverlay.Visibility = Visibility.Visible;

                    await Task.Delay(TimeSpan.FromSeconds(3));

                    MessageOverlay.Visibility = Visibility.Collapsed;
                    break;

            }

        }

        private void FillCountryPathMap()
        {
            var allCountries = continents.SelectMany(c => c.paisos).ToList();
            var countryById = allCountries.ToDictionary(p => p.Id);

            _countryPathMap = MapCanvas.Children
                .OfType<Path>()
                .Where(p => p.Tag != null && long.TryParse(p.Tag.ToString(), out _))
                .GroupBy(p => long.Parse(p.Tag.ToString()))
                .ToDictionary(
                    grp => grp.Key,
                    grp => grp.First()
                );
        }


        private async void OnMapUpdated(List<(long countryId, int troops, long? playerId)> list)
        {

            if (_countryPathMap == null || _countryPathMap.Count == 0)
            {
                await carregarDades();
            }

            Dispatcher.Invoke(() =>
            {
                foreach (var (countryId, troops, playerId) in list)
                {
                    if (_countryPathMap.TryGetValue(countryId, out var path))
                    {
                        if (path.DataContext is Pais country)
                        {
                            country.Tropes = troops;
                            country.PaisDeJugador = playerId.HasValue
                                ? currentPartida.Jugadors.FirstOrDefault(j => j.Id == playerId.Value)
                                : null;

                            Jugador? jugador = null;
                            if (country.PaisDeJugador != null)
                            {
                                jugador = currentPartida.Jugadors
                                    .FirstOrDefault(j => j.Id == country.PaisDeJugador.Id);
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
                            else
                            {
                                path.Fill = Brushes.Gray;
                                if (_countryLabelMap.TryGetValue(countryId, out var lbl))
                                {
                                    lbl.Text = 0 + "";
                                }
                            }



                            if (jugador != null && country != null)
                            {
                                var existingOkupa = currentPartida.Okupa
                                    .FirstOrDefault(o => o.Pais.Id == country.Id);

                                if (existingOkupa != null)
                                {
                                    if (existingOkupa.Jugador.Id == jugador.Id)
                                    {
                                        existingOkupa.Tropes = troops;
                                    }
                                    else
                                    {
                                        currentPartida.Okupa.Remove(existingOkupa);
                                        currentPartida.Okupa.Add(new Okupa
                                        {
                                            Jugador = jugador,
                                            Pais = country,
                                            Tropes = troops
                                        });
                                    }
                                }
                                else
                                {
                                    currentPartida.Okupa.Add(new Okupa
                                    {
                                        Jugador = jugador,
                                        Pais = country,
                                        Tropes = troops
                                    });
                                }

                                jugador.NotifyTroopsChanged();
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

            this.NavigationService?.Navigate(new MenuPage());


        }

        private async void btn_next_turn_click(object sender, RoutedEventArgs e)
        {
            if (currentPartida.EstatPartida == GameState.Attaking)
            {
                await _gameService.SendEndAttackingAsync();
            }
            else if (currentPartida.EstatPartida == GameState.Reforce)
            {
                await _gameService.SendEndTurnAsync();
            }
        }
    }
}