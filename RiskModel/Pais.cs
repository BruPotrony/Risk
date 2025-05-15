using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RiskModel
{
    public class Pais
    {
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Nom { get; set; }

        [JsonPropertyName("continentId")]
        public long ContinentId { get; set; }

        [JsonPropertyName("image")]
        public string Imatge { get; set; }
        public List<Pais> Fronteres { get; set; } = new ();

        public int Tropes { get; set; }
        public Jugador? PaisDeJugador { get; set; } = null;

    }

}
