using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Payments.Core.Services;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace Payments.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private string? _accessToken;
        private DateTime _expiration;

        public TokenService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && _expiration > DateTime.UtcNow)
            {
                return _accessToken;
            }

            var keycloakSection = _configuration.GetSection("Keycloak");
            var tokenUrl = $"{keycloakSection["BaseUrl"]}/realms/{keycloakSection["Realm"]}/protocol/openid-connect/token";

            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", keycloakSection["ClientId"] ?? ""),
                new KeyValuePair<string, string>("client_secret", keycloakSection["ClientSecret"] ?? "")
            });

            var response = await _httpClient.PostAsync(tokenUrl, requestContent);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (tokenResponse == null) throw new Exception("Token response was null");

            _accessToken = tokenResponse.access_token;
            _expiration = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in - 30); 

            return _accessToken;
        }

        private class TokenResponse
        {
            public string access_token { get; set; } = string.Empty;
            public int expires_in { get; set; }
        }
    }
}
