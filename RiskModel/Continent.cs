
using System.Text.Json.Serialization;

namespace RiskModel
{
    public class Continent
    {
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Nom { get; set; }

        [JsonPropertyName("extraTropes")]
        public int ReforcTropes { get; set; }

        [JsonPropertyName("countries")]
        public List<Pais> paisos { get; set; } = new List<Pais>();
    }


}
