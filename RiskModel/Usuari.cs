using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RiskModel
{
    public class Usuari
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("avatar")]
        public Avatar Avatar { get; set; }
        [JsonPropertyName("wins")]
        public int wins { get; set; }
        [JsonPropertyName("games")]
        public int games { get; set; }
    }

}
