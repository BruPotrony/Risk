using RiskModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RiskServerConnection
{
    public class UserService
    {
        private readonly HttpClient _http;

        public UserService(HttpClient http) => _http = http;

        public async Task<Usuari> LoginAsync(string username, string password)
        {
            var creds = $"{username}:{password}";
            var bytes = Encoding.UTF8.GetBytes(creds);
            var base64 = Convert.ToBase64String(bytes);
            _http.DefaultRequestHeaders.Authorization
                       = new AuthenticationHeaderValue("Basic", base64);

            var payload = new { username, password };
            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await _http
                .PostAsync("api/login", content)
                .ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            var respJson = await resp.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

            var user = JsonSerializer.Deserialize<Usuari>(
                respJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return user;
        }








        public async Task<List<Avatar>> GetAllAvatarsAsync()
        {
            var resp = await _http.GetAsync("api/avatars")
                                  .ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync()
                              .ConfigureAwait(false);

            var avatars = JsonSerializer.Deserialize<List<Avatar>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            return avatars ?? new List<Avatar>();
        }

        public async Task<Usuari> RegisterAsync(Usuari user)
        {
            using var client = new HttpClient
            {
                BaseAddress = _http.BaseAddress,
                Timeout = _http.Timeout
            };

            var payload = new
            {
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                username = user.Username,
                password = user.Password,
                avatarId = user.Avatar.Id
            };
            string json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await client
                .PostAsync("api/users/register", content)
                .ConfigureAwait(false);

            resp.EnsureSuccessStatusCode();

            var respJson = await resp.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

            var createdUser = JsonSerializer.Deserialize<Usuari>(
                respJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (createdUser == null)
                throw new InvalidOperationException("No s'ha pogut llegir la resposta");

            return createdUser;
        }



        public async Task<List<Continent>> GetAllContinentsAsync()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var continents = await _http
                .GetFromJsonAsync<List<Continent>>("api/continents", options)
                .ConfigureAwait(false);

            return continents ?? new List<Continent>();
        }

        public async Task<List<Frontera>> GetAllBordersAsync()
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var borders = await _http
                .GetFromJsonAsync<List<Frontera>>("api/borders", options)
                .ConfigureAwait(false);

            return borders ?? new List<Frontera>();
        }


    }

}
