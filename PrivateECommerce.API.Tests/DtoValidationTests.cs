using FluentAssertions;
using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.DTOs.Auth;
using System.ComponentModel.DataAnnotations;

namespace PrivateECommerce.API.Tests
{
    public class DtoValidationTests
    {
        [Fact]
        public void LoginRequiresCredentials()
        {
            var dto = new LoginRequestDto { LoginId = "", Password = "" };

            Validate(dto).Should().NotBeEmpty();
        }

        [Fact]
        public void ResetPasswordRequiresStrongPassword()
        {
            var dto = new ResetPasswordDto { Token = "token", NewPassword = "short" };

            Validate(dto).Should().Contain(r => r.MemberNames.Contains(nameof(ResetPasswordDto.NewPassword)));
        }

        [Fact]
        public void CartUpdateRejectsNegativeQuantity()
        {
            var dto = new UpdateCartQuantityDto
            {
                ProductVariantId = 1,
                Quantity = -1
            };

            Validate(dto).Should().Contain(r => r.MemberNames.Contains(nameof(UpdateCartQuantityDto.Quantity)));
        }

        [Fact]
        public void OrderPlacementRequiresItems()
        {
            var dto = new PlaceOrderByCustomerDto();

            Validate(dto).Should().Contain(r => r.MemberNames.Contains(nameof(PlaceOrderByCustomerDto.Items)));
        }

        [Fact]
        public void ProductCreationRequiresVariant()
        {
            var dto = new AdminCreateProductDto
            {
                Name = "Suture",
                Description = "Medical product",
                CategoryId = 1,
                BrandId = 1
            };

            Validate(dto).Should().Contain(r => r.MemberNames.Contains(nameof(AdminCreateProductDto.Variants)));
        }

        private static List<ValidationResult> Validate(object model)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(
                model,
                new ValidationContext(model),
                results,
                validateAllProperties: true);

            return results;
        }
    }
}
