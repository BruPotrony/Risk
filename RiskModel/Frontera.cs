using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RiskModel
{
    public class Frontera
    {
        [JsonPropertyName("country1")]
        public long Pais1Id { get; set; }
        [JsonPropertyName("country2")]
        public long Pais2Id { get; set; }
    }
}
