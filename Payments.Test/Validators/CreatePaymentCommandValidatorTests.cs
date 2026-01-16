using FluentAssertions;
using Payments.Application.Commands;
using Payments.Application.Validators;
using System;
using Xunit;

namespace Payments.Test.Validators
{
    public class CreatePaymentCommandValidatorTests
    {
        private readonly CreatePaymentCommandValidator _validator;

        public CreatePaymentCommandValidatorTests()
        {
            _validator = new CreatePaymentCommandValidator();
        }

        [Fact]
        public void Validate_ShouldBeValid_WhenAllFieldsAreCorrect()
        {
            // Arrange
            var command = new CreatePaymentCommand
            {
                BookingId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Total = 100.0m,
                Currency = "USD"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenBookingIdIsEmpty()
        {
            // Arrange
            var command = new CreatePaymentCommand
            {
                BookingId = Guid.Empty,
                UserId = Guid.NewGuid(),
                Total = 100.0m,
                Currency = "USD"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.PropertyName == "BookingId");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenUserIdIsEmpty()
        {
            // Arrange
            var command = new CreatePaymentCommand
            {
                BookingId = Guid.NewGuid(),
                UserId = Guid.Empty,
                Total = 100.0m,
                Currency = "USD"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.PropertyName == "UserId");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenTotalIsZeroOrLess()
        {
            // Arrange
            var command = new CreatePaymentCommand
            {
                BookingId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Total = 0,
                Currency = "USD"
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.PropertyName == "Total");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenCurrencyIsInvalid()
        {
            // Arrange
            var command = new CreatePaymentCommand
            {
                BookingId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Total = 100.0m,
                Currency = "US" // Too short
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.PropertyName == "Currency");
        }
    }
}
