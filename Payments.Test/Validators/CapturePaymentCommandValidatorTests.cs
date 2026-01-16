using FluentAssertions;
using Payments.Application.Commands;
using Payments.Application.Validators;
using Xunit;

namespace Payments.Test.Validators
{
    public class CapturePaymentCommandValidatorTests
    {
        private readonly CapturePaymentCommandValidator _validator;

        public CapturePaymentCommandValidatorTests()
        {
            _validator = new CapturePaymentCommandValidator();
        }

        [Fact]
        public void Validate_ShouldBeValid_WhenStripeIdIsPresent()
        {
            // Arrange
            var command = new CapturePaymentCommand { StripeId = "pi_123" };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenStripeIdIsEmpty()
        {
            // Arrange
            var command = new CapturePaymentCommand { StripeId = "" };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.PropertyName == "StripeId");
        }
    }
}
