using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
using System;

namespace Payments.Test.Services
{
    public class UserAuditServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<UserAuditService>> _loggerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly HttpClient _httpClient;
        private readonly UserAuditService _userAuditService;

        public UserAuditServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://users-service/")
            };

            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["UsersUrl"]).Returns("http://users-service/");

            _loggerMock = new Mock<ILogger<UserAuditService>>();
            _tokenServiceMock = new Mock<ITokenService>();
            _tokenServiceMock.Setup(t => t.GetTokenAsync()).ReturnsAsync("mock-token");

            _userAuditService = new UserAuditService(_httpClient, _configurationMock.Object, _loggerMock.Object, _tokenServiceMock.Object);
        }

        [Fact]
        public async Task AuditAsync_ShouldPostCorrectPayload_WithAuthHeader()
        {
            // Arrange
            var audit = new UserAuditDto { Title = "Test Action", JsonData = "{}" };

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
            await _userAuditService.AuditAsync(audit);

            // Assert
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("users/AddAudit") &&
                    req.Headers.Authorization.Scheme == "Bearer" &&
                    req.Headers.Authorization.Parameter == "mock-token"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task AuditAsync_ShouldLogWarning_WhenResponseIsNotSuccess()
        {
            // Arrange
            var audit = new UserAuditDto { Title = "Test Action", JsonData = "{}" };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Bad Request")
                });

            // Act
            await _userAuditService.AuditAsync(audit);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Fallo al registrar auditoria")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
