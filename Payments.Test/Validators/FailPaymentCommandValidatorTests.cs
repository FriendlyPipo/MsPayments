using FluentAssertions;
using Payments.Application.Commands;
using Payments.Application.Validators;
using Xunit;

namespace Payments.Test.Validators
{
    public class FailPaymentCommandValidatorTests
    {
        private readonly FailPaymentCommandValidator _validator;

        public FailPaymentCommandValidatorTests()
        {
            _validator = new FailPaymentCommandValidator();
        }

        [Fact]
        public void Validate_ShouldBeValid_WhenStripeIdIsPresent()
        {
            // Arrange
            var command = new FailPaymentCommand { StripeId = "pi_123" };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenStripeIdIsEmpty()
        {
            // Arrange
            var command = new FailPaymentCommand { StripeId = "" };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.PropertyName == "StripeId");
        }
    }
}
