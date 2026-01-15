using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Payments.Core.Dtos;
using Payments.Core.Services;
using System.Threading.Tasks;
using System;
using System.Net.Http;

namespace Payments.Infrastructure.Services
{
    public class UserAuditService : IUserAuditService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserAuditService> _logger;
        private readonly ITokenService _tokenService;

        public UserAuditService(HttpClient httpClient, IConfiguration configuration, ILogger<UserAuditService> logger, ITokenService tokenService)
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

        public async Task AuditAsync(UserAuditDto audit)
        {
            try
            {
                var token = await _tokenService.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync("users/AddAudit", audit);
                
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Fallo al registrar auditoria. Status Code: {response.StatusCode}. Detalle: {content}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Registrando Auditoria de Usuario");
            }
        }
    }
}
