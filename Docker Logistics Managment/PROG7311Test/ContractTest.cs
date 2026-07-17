using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using PROG7311.Controllers;
using PROG7311.Models;

namespace PROG7311.Tests
{
    public class ContractTest
    {
        [Fact]
        public async Task Create_WithValidPdf_ShouldRedirectToIndex()
        {
            // Arrange
            var context = TestHelpers.GetDbContext();

            context.Clients.Add(new Client
            {
                ClientId = 1,
                Name = "Test Client",
                ContactDetails = "test@test.com",
                Region = "USA"
            });

            await context.SaveChangesAsync();

            var controller = new ContractsController(TestHelpers.GetHttpClientFactory(context));

            // Fake HttpContext + Session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new FakeSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var fileContent = new MemoryStream(new byte[] { 1, 2, 3, 4 });

            IFormFile pdfFile = new FormFile(fileContent, 0, fileContent.Length, "PdfFile", "contract.pdf")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };

            var contract = new Contract
            {
                ClientId = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1),
                Status = "Active",
                ServiceLevel = "1-Road",
                PdfFile = pdfFile
            };

            // Act
            var result = await controller.Create(contract, false);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }


        [Fact]
        public async Task Create_WithNonPdfFile_ShouldReturnViewWithModelError()
        {
            // Arrange
            var context = TestHelpers.GetDbContext();

            context.Clients.Add(new Client
            {
                ClientId = 1,
                Name = "Test Client",
                ContactDetails = "test@test.com",
                Region = "USA"
            });

            await context.SaveChangesAsync();

            var controller = new ContractsController(TestHelpers.GetHttpClientFactory(context));

            // Fake HttpContext + Session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new FakeSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var fileContent = new MemoryStream(new byte[] { 1, 2, 3, 4 });

            IFormFile wrongFile = new FormFile(fileContent, 0, fileContent.Length, "PdfFile", "contract.txt")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };

            var contract = new Contract
            {
                ClientId = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1),
                Status = "Active",
                ServiceLevel = "1-Road",
                PdfFile = wrongFile
            };

            // Act
            var result = await controller.Create(contract, false);

            // Assert
            Assert.False(controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }
    }
}
