using System;
using System.Collections.Generic;
using System.Text;

namespace PROG7311.Tests
{
    [Fact]
    public async Task CreateContract_WithPdfFile_ShouldSucceed()
    {
        // Arrange
        var mockContext = TestHelpers.GetDbContext();
        var controller = new ContractsController(mockContext);

        var contract = new Contract
        {
            ClientId = 1,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(12),
            Status = "Active",
            ServiceLevel = "Road"
        };

        var content = new MemoryStream(new byte[10]);
        var file = new FormFile(content, 0, content.Length, "pdfFile", "contract.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        // Act
        var result = await controller.Create(contract, file);

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
    }
}
