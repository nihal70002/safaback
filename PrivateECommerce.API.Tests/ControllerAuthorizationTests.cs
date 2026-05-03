using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using PrivateECommerce.API.Controllers;
using System.Reflection;

namespace PrivateECommerce.API.Tests
{
    public class ControllerAuthorizationTests
    {
        [Theory]
        [InlineData(typeof(ProductsController), "BulkCreateProducts")]
        [InlineData(typeof(ProductsController), "UpdateProduct")]
        [InlineData(typeof(ProductsController), "DeleteProduct")]
        [InlineData(typeof(BrandController), "AddBrand")]
        [InlineData(typeof(UploadController), "UploadImage")]
        public void PublicWriteEndpointsRequireAdmin(Type controllerType, string actionName)
        {
            var method = controllerType.GetMethods()
                .Single(m => m.Name == actionName);

            var authorize = method.GetCustomAttribute<AuthorizeAttribute>();

            authorize.Should().NotBeNull();
            authorize!.Roles.Should().Be("Admin");
        }

        [Fact]
        public void AdminReportsControllerRequiresAdmin()
        {
            var authorize = typeof(AdminReportsController)
                .GetCustomAttribute<AuthorizeAttribute>();

            authorize.Should().NotBeNull();
            authorize!.Roles.Should().Be("Admin");
        }
    }
}
