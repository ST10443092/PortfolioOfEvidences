using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using PROG7311.Controllers;
using PROG7311.Models;

namespace PROG7311.Tests
{
    public class LoginTest
    {
        [Fact]
        public async Task Index_WithClientRole_ShouldRedirectToClientService()
        {
            // Arrange
            var context = TestHelpers.GetDbContext();

            context.Clients.Add(new Client
            {
                ClientId = 1,
                Name = "client1",
                ContactDetails = "client1@test.com",
                Region = "USA"
            });

            context.Users.Add(new Users
            {
                UsersId = 1,
                Username = "client1",
                Password = "123",
                Role = "Client",
                ClientId = 1
            });

            context.SaveChanges();

            var controller = new LoginController(TestHelpers.GetHttpClientFactory(context));

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new FakeSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await controller.Index("client1", "123");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("ClientService", redirectResult.ControllerName);
        }
    }
}
