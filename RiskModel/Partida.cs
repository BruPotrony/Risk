using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RiskModel
{
    public class Partida
    {
        public int Id { get; set; }
        [JsonPropertyName("gameName")]
        public string Nom { get; set; }
        [JsonPropertyName("isPublic")]
        public bool isPublic { get; set; }
        [JsonPropertyName("token")]
        public string Token { get; set; }
        [JsonPropertyName("maxPlayers")]
        public int maxPlayers { get; set; }
        [JsonPropertyName("currentPlayers")]
        public int currentPlayers { get; set; }
        public List<Jugador> Jugadors { get; set; }
        public List<Okupa> Okupa { get; set; }
        [JsonIgnore]
        public Jugador Admin { get; set; }
        [JsonIgnore]
        public Jugador TornPlayer { get; set; }
        [JsonIgnore]
        public GameState EstatPartida { get; set; } = GameState.NotStarted;



        [JsonIgnore]
        public bool isCreator { get; set; }
    }

}
