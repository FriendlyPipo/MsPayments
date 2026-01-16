using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Payments.Core.Dtos;
using Payments.Core.Services;
using System.Net.Http.Headers;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Payments.Infrastructure.Services
{
    public class UserLogService : IUserLogService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserLogService> _logger;
        private readonly ITokenService _tokenService;

        public UserLogService(HttpClient httpClient, IConfiguration configuration, ILogger<UserLogService> logger, ITokenService tokenService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _tokenService = tokenService;
            
            var baseUrl = configuration["UsersUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                _httpClient.BaseAddress = new Uri(baseUrl);
            }
        }

        public async Task LogAsync(UserLogDto logDto)
        {
            try
            {
                // Obtener token
                var token = await _tokenService.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // endpoint Users
                var response = await _httpClient.PostAsJsonAsync("users/AddLog", logDto);
                
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Fallo al registrar log. Status Code: {response.StatusCode}. Detalle: {content}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Registrando Log de Usuario en Payments");
            }
        }
    }
}
