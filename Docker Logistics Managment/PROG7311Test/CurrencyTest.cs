using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using PROG7311.Controllers;
using PROG7311.Models;
using PROG7311.Services;

namespace PROG7311.Tests
{
    public class CurrencyTest
    {
        [Fact]
        public async Task Create_ServiceRequest_ShouldConvertUsdToZar()
        {
            // Arrange
            var context = TestHelpers.GetDbContext();

            context.Clients.Add(new Client
            {
                ClientId = 1,
                Name = "Test Client",
                ContactDetails = "client@test.com",
                Region = "USA"
            });

            context.Contracts.Add(new Contract
            {
                ContractId = 1,
                ClientId = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1),
                Status = "Active",
                ServiceLevel = "1-Road"
            });

            await context.SaveChangesAsync();

            var mockCurrencyService = new Mock<ICurrencyConversionService>();

            mockCurrencyService
                .Setup(x => x.ConvertToZarAsync(100m, "USD", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CurrencyConversionResult(
                    1900m, // Amount in ZAR
                    19m,   // Exchange rate
                    DateOnly.FromDateTime(DateTime.UtcNow),
                    "USD"
                ));

            var controller = new ServiceRequestsController(TestHelpers.GetHttpClientFactory(context), mockCurrencyService.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new FakeSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var request = new ServiceRequest
            {
                ClientId = 1,
                Description = "Test service request",
                Cost = 100m,
                Status = "In Progress"
            };

            // Act
            var result = await controller.Create(request, false);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var savedRequest = await context.ServiceRequests.FindAsync(1);
            Assert.NotNull(savedRequest);
            Assert.Equal("USD", savedRequest!.CostCurrencyCode);
            Assert.Equal(1900m, savedRequest.CostInZar);
            Assert.Equal(19m, savedRequest.ExchangeRateToZar);
        }
    }
}
