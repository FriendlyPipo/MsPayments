using FluentAssertions;
using Moq;
using Payments.Application.Commands;
using Payments.Application.Handlers.Commands;
using Payments.Core.Repositories;
using Payments.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Payments.Test.Handlers.Commands
{
    public class CreateInvoiceCommandHandlerTests
    {
        private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
        private readonly CreateInvoiceCommandHandler _handler;

        public CreateInvoiceCommandHandlerTests()
        {
            _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            _handler = new CreateInvoiceCommandHandler(_invoiceRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenInvoiceIsCreatedSuccessfully()
        {
            // Arrange
            var request = new CreateInvoiceCommand
            {
                PaymentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Total = 120.50m,
                Currency = "USD",
                UserName = "Jane Doe",
                UserEmail = "jane@example.com"
            };

            _invoiceRepositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<Invoice>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            _invoiceRepositoryMock.Verify(r => r.CreateAsync(It.Is<Invoice>(i => 
                i.PaymentId.Value == request.PaymentId &&
                i.UserId.Value == request.UserId &&
                i.Total.Value == request.Total &&
                i.Currency.Value == request.Currency &&
                i.UserName == request.UserName &&
                i.UserEmail == request.UserEmail
            )), Times.Once);
        }
    }
}
