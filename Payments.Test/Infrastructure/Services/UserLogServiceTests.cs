using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Payments.Core.Dtos;
using Payments.Core.Services;
using Payments.Infrastructure.Services;
using Xunit;
using FluentAssertions;
using System;

namespace Payments.Test.Infrastructure.Services
{
    public class UserLogServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<UserLogService>> _loggerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly HttpClient _httpClient;
        private readonly UserLogService _userLogService;

        public UserLogServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://users-service/")
            };

            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["UsersUrl"]).Returns("http://users-service/");

            _loggerMock = new Mock<ILogger<UserLogService>>();
            _tokenServiceMock = new Mock<ITokenService>();
            _tokenServiceMock.Setup(t => t.GetTokenAsync()).ReturnsAsync("mock-token");

            _userLogService = new UserLogService(_httpClient, _configurationMock.Object, _loggerMock.Object, _tokenServiceMock.Object);
        }

        [Fact]
        public async Task LogAsync_ShouldPostCorrectPayload_WithAuthHeader()
        {
            // Arrange
            var logDto = new UserLogDto { UserId = Guid.NewGuid(), Title = "Test Title", Description = "Test Desc" };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            await _userLogService.LogAsync(logDto);

            // Assert
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("users/AddLog") &&
                    req.Headers.Authorization.Scheme == "Bearer" &&
                    req.Headers.Authorization.Parameter == "mock-token"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
