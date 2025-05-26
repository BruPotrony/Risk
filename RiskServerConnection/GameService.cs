using RiskModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RiskServerConnection
{
    public class GameService : IDisposable
    {
        private readonly IGameWebSocketService _ws;

        private bool _registered;
        private CancellationTokenSource _cts;

        public GameService(IGameWebSocketService ws)
            => _ws = ws ?? throw new ArgumentNullException(nameof(ws));

        public async Task RegisterAsync(long userId)
        {
            string resp = await _ws.ReceiveAsync();
            var payload = new Dictionary<string, object>
            {
                ["action"] = "register",
                ["userId"] = userId
            };
            string json = JsonSerializer.Serialize(payload);
            await _ws.SendAsync(json);


            string resp1 = await _ws.ReceiveAsync();
            _registered = true;
            Debug.WriteLine($"Registered: {resp}");
        }
        public async Task<Partida> CreateGameAsync(Partida partida)
        {
            if (!_registered)
                throw new InvalidOperationException("S'ha de registrar abans de crear la partida");

            var payload = new Dictionary<string, object>
            {
                ["action"] = "create_game",
                ["gameName"] = partida.Nom,
                ["isPublic"] = partida.isPublic,
                ["maxPlayers"] = partida.maxPlayers,
            };
            string jsonReq = JsonSerializer.Serialize(payload);
            await _ws.SendAsync(jsonReq);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            while (true)
            {
                string msg = await _ws.ReceiveAsync();
                using var doc = JsonDocument.Parse(msg);
                var root = doc.RootElement;

                if (root.TryGetProperty("action", out var act)
                 && act.GetString() == "game_created")
                {
                    var partidaCreada = JsonSerializer.Deserialize<Partida>(msg, options);
                    if (partidaCreada == null)
                        throw new InvalidOperationException("No s'ha pogut deserialitzar la partida rebuda.");

                    partidaCreada.Jugadors = new List<Jugador>();
                    return partidaCreada;
                }

            }
        }





        public async Task<List<Partida>> ListGamesAsync()
        {
            if (_ws.State != WebSocketState.Open)
                throw new InvalidOperationException("WebSocket no connectat.");

            var req = new { action = "list_games" };
            await _ws.SendAsync(JsonSerializer.Serialize(req));

            while (true)
            {
                string respJson = await _ws.ReceiveAsync();
                using var doc = JsonDocument.Parse(respJson);
                var root = doc.RootElement;

                if (root.TryGetProperty("action", out var act)
                    && act.GetString() == "games_list")
                {
                    if (!root.TryGetProperty("games", out var gamesElem)
                     || gamesElem.ValueKind != JsonValueKind.Array)

                        return new List<Partida>();

                    var list = new List<Partida>();
                    foreach (var gameEl in gamesElem.EnumerateArray())
                    {
                        list.Add(new Partida
                        {
                            Id = gameEl.GetProperty("id").GetInt32(),
                            Token = gameEl.GetProperty("token").GetString(),
                            currentPlayers = gameEl.GetProperty("players").GetInt32(),
                            maxPlayers = gameEl.GetProperty("maxPlayers").GetInt32(),
                            Nom = gameEl.GetProperty("gameName").GetString()
                        });
                    }
                    return list;
                }
            }
        }


        public async Task<string> LeaveGameAsync()
        {
            if (!_registered)
                throw new InvalidOperationException("Has de registrar-te abans de sortir d'una partida.");

            var payload = new Dictionary<string, object>
            {
                ["action"] = "leave_game"
            };
            string json = JsonSerializer.Serialize(payload);
            await _ws.SendAsync(json);

            string respJson = await _ws.ReceiveAsync();
            return respJson;
        }


        public async Task<string> JoinGameAsync(string token)
        {
            if (!_registered)
                throw new InvalidOperationException("\"S'ha de registrar abans de crear la partida");

            var payload = new Dictionary<string, object>
            {
                ["action"] = "join_game",
                ["token"] = token
            };

            string reqJson = JsonSerializer.Serialize(payload);
            await _ws.SendAsync(reqJson);

            string respJson = await _ws.ReceiveAsync();

            return respJson;
        }


        public event Action<List<long>> PlayerListReceived;
        public event Action<long> IdTornRecived;
        public event Action<long> attackInitializedRecived;
        public event Action<long> IdPlayerLeftRecived;
        public event Action<GameState> gameStateRecived;
        public event Action<List<(long countryId, int troops, long? playerId)>> MapUpdatedRecived;
        public event Action<List<int>, List<int>> DiceAttackRecived;
        public event Action territoryConqueredRecived;
        public event Action<long, long> territoryUnderAttackRecived;
        public event Action<long> totalTroopsToPlaceRecived;
        public event Action winRecived;




        private bool _listening;


        public void StartListening()
        {
            if (_listening) return;
            _listening = true;
            _cts = new CancellationTokenSource();
            _ = ReceiveLoopAsync();
        }

        public void StopListening()
        {
            if (!_listening) return;
            _listening = false;
            _cts.Cancel();
        }

        bool someoneHasWon = false;

        private async Task ReceiveLoopAsync()
        {
            while (_ws.State == WebSocketState.Open)
            {
                string msg = await _ws.ReceiveAsync();
                using var doc = JsonDocument.Parse(msg);
                var root = doc.RootElement;
                if (!root.TryGetProperty("action", out var act)) continue;

                switch (act.GetString())
                {
                    case "player_list":
                        var ids = new List<long>();
                        foreach (var el in root.GetProperty("players").EnumerateArray())
                            ids.Add(el.GetInt64());
                        PlayerListReceived?.Invoke(ids);
                        break;

                    case "player_turn":
                        long id;
                        id = root.GetProperty("playerId").GetInt64();
                        IdTornRecived?.Invoke(id);
                        break;

                    case "player_left":
                        long idPlayerLeft;
                        idPlayerLeft = root.GetProperty("player_id").GetInt64();
                        IdPlayerLeftRecived?.Invoke(idPlayerLeft);
                        break;

                    case "attack_initiated":
                        long defenderCountryId;
                        defenderCountryId = root.GetProperty("targetCountryId").GetInt64();
                        attackInitializedRecived?.Invoke(defenderCountryId);
                        break;

                    case "territory_under_attack":
                        long defendeCountryId;
                        long attackCountryId;
                        attackCountryId = root.GetProperty("sourceCountryId").GetInt64();
                        defendeCountryId = root.GetProperty("targetCountryId").GetInt64();
                        attackInitializedRecived?.Invoke(defendeCountryId);
                        territoryUnderAttackRecived?.Invoke(attackCountryId, defendeCountryId);
                        break;

                    case "territory_conquered":
                        territoryConqueredRecived?.Invoke();
                        break;

                    case "bonus":
                        long totalTroops;
                        totalTroops = root.GetProperty("bonusTroops").GetInt64();
                        totalTroopsToPlaceRecived?.Invoke(totalTroops);
                        break;

                    case "bonus_to_place":
                        long totalTroopsToPlace;
                        totalTroopsToPlace = root.GetProperty("totalTroopsToPlace").GetInt64();
                        totalTroopsToPlaceRecived?.Invoke(totalTroopsToPlace);
                        break;

                    case "troops_placed":
                        long total;
                        total = root.GetProperty("remainingTroops").GetInt64();
                        totalTroopsToPlaceRecived?.Invoke(total);
                        break;

                    case "error":
                        string errorMessage;
                        errorMessage = root.GetProperty("message").GetString();
                        throw new InvalidOperationException($"Error: {errorMessage}");
                        break;

                    case "win":
                        if (someoneHasWon) break;
                        someoneHasWon = true;
                        winRecived?.Invoke();
                        break;


                    case "dice_rolls":
                        if (root.TryGetProperty("attackerDice", out var attackerElem)
                         && attackerElem.ValueKind == JsonValueKind.Array
                         && root.TryGetProperty("defenderDice", out var defenderElem)
                         && defenderElem.ValueKind == JsonValueKind.Array)
                        {
                            var dausAtacants = attackerElem
                                .EnumerateArray()
                                .Select(d => d.GetInt32())
                                .ToList();

                            var dausDefensors = defenderElem
                                .EnumerateArray()
                                .Select(d => d.GetInt32())
                                .ToList();

                            DiceAttackRecived?.Invoke(dausAtacants, dausDefensors);
                        }
                        break;


                    case "stage_change":
                        string stage;
                        stage = root.GetProperty("stage").GetString();
                        switch (stage)
                        {
                            case "ATTACKING":
                                gameStateRecived?.Invoke(GameState.Attaking);
                                break;
                            case "OCCUPATION":
                                gameStateRecived?.Invoke(GameState.Occupation);
                                break;
                            case "REFORCE":
                                gameStateRecived?.Invoke(GameState.Reforce);
                                break;
                            case "BONUS":
                                gameStateRecived?.Invoke(GameState.Bonus);
                                break;
                        }
                        break;

                    case "map_update":
                        if (root.TryGetProperty("countries", out var countriesElem)
                            && countriesElem.ValueKind == JsonValueKind.Array)
                        {
                            var llista = new List<(long countryId, int troops, long? playerId)>();
                            foreach (var el in countriesElem.EnumerateArray())
                            {
                                long countryId = el.GetProperty("countryId").GetInt64();
                                int troops = el.GetProperty("troops").GetInt32();

                                long? playerId = null;
                                if (el.TryGetProperty("playerId", out var pidElem)
                                    && pidElem.ValueKind == JsonValueKind.Number)
                                {
                                    playerId = pidElem.GetInt64();
                                }

                                llista.Add((countryId, troops, playerId));
                            }
                            MapUpdatedRecived?.Invoke(llista);
                        }
                        break;

                }
            }
        }


        public async Task SendOccupationAsync(long countryId, int troops)
        {
            var payload = new
            {
                action = "send_input",
                data = new
                {
                    type = "occupation",
                    countryId = countryId,
                    troops = troops
                }
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            string json = JsonSerializer.Serialize(payload, options);
            await _ws.SendAsync(json);

            
        }


        public async Task SendBonusAsync(long countryId, int troops)
        {
            var payload = new
            {
                action = "send_input",
                data = new
                {
                    type = "place_troops",
                    countryId = countryId,
                    troops = troops
                }
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            string json = JsonSerializer.Serialize(payload, options);
            await _ws.SendAsync(json);


        }


        public async Task SendAttackAsync(long countryId, long enemyCountryId, int troops)
        {
            var payload = new
            {
                action = "send_input",
                data = new
                {
                    type = "attack",
                    countryId,
                    enemyCountryId,
                    troops
                }
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string json = JsonSerializer.Serialize(payload, options);
            await _ws.SendAsync(json);
        }


        public async Task SendMoveTroopsAsync(int troops)
        {
            var payload = new
            {
                action = "send_input",
                data = new
                {
                    type = "move_troops",
                    troops = troops
                }
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string json = JsonSerializer.Serialize(payload, options);
            await _ws.SendAsync(json);
        }


        public async Task SendEndAttackingAsync()
        {
            var payload = new
            {
                action = "send_input",
                data = new
                {
                    type = "end_attack"
                }
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string json = JsonSerializer.Serialize(payload, options);
            await _ws.SendAsync(json);
        }


        public async Task SendFortifyAsync(long sourceCountryId, long targetCountryId, int troops)
        {
            var payload = new
            {
                action = "send_input",
                data = new
                {
                    type = "fortify",
                    sourceCountryId = sourceCountryId,
                    targetCountryId = targetCountryId,
                    troops = troops
                }
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string json = JsonSerializer.Serialize(payload, options);
            await _ws.SendAsync(json);

            string resp = await _ws.ReceiveAsync();
            string resp1 = await _ws.ReceiveAsync();
            Debug.WriteLine(resp1);

        }



        public async Task SendEndTurnAsync()
        {
            var payload = new
            {
                action = "send_input",
                data = new
                {
                    type = "end_turn"
                }
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string json = JsonSerializer.Serialize(payload, options);
            await _ws.SendAsync(json);
        }

        public void Dispose() => _ws.Dispose();


    }
}