using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Payments.Infrastructure.Services;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;

namespace Payments.Test.Infrastructure.Services
{
    public class TokenServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly HttpClient _httpClient;
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

            _configurationMock = new Mock<IConfiguration>();
            var keycloakSection = new Mock<IConfigurationSection>();
            keycloakSection.Setup(s => s["BaseUrl"]).Returns("http://keycloak");
            keycloakSection.Setup(s => s["Realm"]).Returns("test-realm");
            keycloakSection.Setup(s => s["ClientId"]).Returns("test-client");
            keycloakSection.Setup(s => s["ClientSecret"]).Returns("test-secret");

            _configurationMock.Setup(c => c.GetSection("Keycloak")).Returns(keycloakSection.Object);

            _tokenService = new TokenService(_httpClient, _configurationMock.Object);
        }

        [Fact]
        public async Task GetTokenAsync_ShouldReturnToken_AndCacheIt()
        {
            // Arrange
            var expectedToken = "test-access-token";
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new { access_token = expectedToken, expires_in = 3600 })
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var token1 = await _tokenService.GetTokenAsync();
            var token2 = await _tokenService.GetTokenAsync();

            // Assert
            token1.Should().Be(expectedToken);
            token2.Should().Be(expectedToken);

            // Verify call was only made once (caching)
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
