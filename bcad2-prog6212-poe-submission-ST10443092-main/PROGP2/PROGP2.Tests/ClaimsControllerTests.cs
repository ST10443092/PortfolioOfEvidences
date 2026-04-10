using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using PROGP2.Controllers;
using PROGP2.Data;
using PROGP2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PROGP2.Tests
{
    public class ClaimsControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ClaimsController _controller;

        public ClaimsControllerTests()
        {
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // fresh DB for each test
                .Options;

            _context = new ApplicationDbContext(options);

            // Seed a test user and claim
            _context.Users.Add(new User
            {
                UserID = 1,
                Name = "TestUser",
                UserName = "testuser", 
                Password = "test123",
                Role = "Employee"
            });
            _context.Claims.Add(new Claim { ID = 1, UserID = 1, Status = "Pending", Hours = 5, Rate = 100 });
            _context.SaveChanges();

            // Initialize the controller
            _controller = new ClaimsController(_context);

            // Mock an HttpContext with a fake session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession(); // helper class below
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
            // Add mock TempData
            _controller.TempData = new TempDataDictionary(
                _controller.HttpContext,
                Mock.Of<ITempDataProvider>()
            );
        }

        // ----------------------------
        // Helper mock Session for tests
        // ----------------------------
        private class TestSession : ISession
        {
            private readonly Dictionary<string, byte[]> _sessionStorage = new();
            public bool IsAvailable => true;
            public string Id => "TestSession";
            public IEnumerable<string> Keys => _sessionStorage.Keys;
            public void Clear() => _sessionStorage.Clear();
            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public void Remove(string key) => _sessionStorage.Remove(key);
            public void Set(string key, byte[] value) => _sessionStorage[key] = value;
            public bool TryGetValue(string key, out byte[] value) => _sessionStorage.TryGetValue(key, out value);
        }

        // ----------------------------------
        //  TESTS BEGIN BELOW
        // ----------------------------------

        [Fact]
        public async Task Index_AsEmployee_ReturnsOnlyOwnClaims()
        {
            // Arrange
            _controller.HttpContext.Session.SetString("Role", "Employee");
            _controller.HttpContext.Session.SetString("UserID", "1");

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Claim>>(viewResult.Model);

            Assert.Single(model); // should only return the one claim belonging to user 1
            Assert.Equal(1, model.First().UserID);
        }

        [Fact]
        public async Task Verify_NonCoordinator_ReturnsRedirectToIndex()
        {
            // Arrange
            _controller.HttpContext.Session.SetString("Role", "Employee");

            // Act
            var result = await _controller.Verify(1);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Claims", redirect.ControllerName);
        }

        [Fact]
        public async Task Approve_InvalidStatus_ReturnsRedirectToDetails()
        {
            // Arrange
            _controller.HttpContext.Session.SetString("Role", "Manager");

            var claim = _context.Claims.First();
            claim.Status = "Pending";
            _context.SaveChanges();

            // Act
            var result = await _controller.Approve(1);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
        }

        [Fact]
        public async Task DownloadPdf_ReturnsFileContentResult_WhenPdfExists()
        {
            // Arrange
            var claim = _context.Claims.First();
            claim.RatePDF = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // mock PDF bytes
            _context.SaveChanges();

            // Act
            var result = await _controller.DownloadPdf(1);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
        }
    }
}
