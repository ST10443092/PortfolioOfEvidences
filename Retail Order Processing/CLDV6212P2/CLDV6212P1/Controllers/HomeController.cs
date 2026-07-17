using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Mvc;
using CLDV6212P1.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
//using CLDV6212P1.Models;

namespace CLDV6212P1.Controllers
{
    public class HomeController : Controller
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly TableServiceClient _tableServiceClient;
        private readonly TableClient _tableClient;
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;

            // Blob service client
            _blobServiceClient = new BlobServiceClient(
                _configuration["AzureStorage:ConnectionString"]
            );

            // Table service client
            _tableServiceClient = new TableServiceClient(
                _configuration["AzureStorage:ConnectionString"]
            );

            // Table client for a specific table (Products)
            _tableClient = _tableServiceClient.GetTableClient("Products");

            // Optional: Create table if it doesn't exist
            _tableClient.CreateIfNotExists();
        }

        // GET: Home/Index
        public async Task<IActionResult> Index()
        {
            
            
            
            var tableClient = _tableServiceClient.GetTableClient("Products");
            await tableClient.CreateIfNotExistsAsync();



            var products = tableClient.Query<Product>().ToList();
            return View(products);
        }

        // GET: Home/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Home/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product,
     IFormFile imageFile,
     IFormFile contractFile)
        {
           // if (ModelState.IsValid)
           // {
                // Validate contract file
                if (contractFile == null || contractFile.Length == 0)
                {
                    ModelState.AddModelError("ContractFile", "Supplier contract is required");
                    return View(product);
                }

                // Validate file type (PDF)
                if (Path.GetExtension(contractFile.FileName).ToLower() != ".pdf")
                {
                    ModelState.AddModelError("ContractFile", "Only PDF files are allowed for contracts");
                    return View(product);
                }

                // Upload product image
                if (imageFile != null && imageFile.Length > 0)
                {
                    product.ImageUrl = await UploadFileToBlob(imageFile, "product-images");
                }

                // Upload contract to Azure Files
                product.ContractUrl = await UploadFileToAzureFiles(contractFile, "contracts");

            // Save product to table storage
            product.PartitionKey = "Products"; // Or category/region/etc.
            product.RowKey = Guid.NewGuid().ToString(); // Unique ID
            var tableClient = _tableServiceClient.GetTableClient("Products");
                await tableClient.AddEntityAsync(product);

                return RedirectToAction(nameof(Index));
           // }
           // return View(product);
        }

        // GET: Home/Edit/5
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var tableClient = _tableServiceClient.GetTableClient("Products");
            var product = await tableClient.GetEntityAsync<Product>(partitionKey, rowKey);
            return View(product.Value);
        }

        // POST: Home/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile imageFile)
        {
          //  if (ModelState.IsValid)
           // {
                // Handle image update
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        var oldBlobUri = new Uri(product.ImageUrl);
                        var oldBlobName = oldBlobUri.Segments.Last();
                        var containerclient = _blobServiceClient.GetBlobContainerClient("product");
                        var oldBlobClient = containerclient.GetBlobClient(oldBlobName);
                        await oldBlobClient.DeleteIfExistsAsync();
                    }

                    // Upload new image
                    var containerClient = _blobServiceClient.GetBlobContainerClient("product");
                    var blobName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                    var blobClient = containerClient.GetBlobClient(blobName);

                    using (var stream = imageFile.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, true);
                    }

                    product.ImageUrl = blobClient.Uri.ToString();
                }

                // Update product in table storage
                var tableClient = _tableServiceClient.GetTableClient("Products");
                await tableClient.UpsertEntityAsync(product);

                return RedirectToAction(nameof(Index));
           // }
            ///return View(product);
        }

        // GET: Home/Delete/5
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var tableClient = _tableServiceClient.GetTableClient("Products");
            var product = await tableClient.GetEntityAsync<Product>(partitionKey, rowKey);
            return View(product.Value);
        }

        // POST: Home/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            var tableClient = _tableServiceClient.GetTableClient("Products");

            // Get product first to handle image deletion
            var product = await tableClient.GetEntityAsync<Product>(partitionKey, rowKey);

            // Delete image from blob storage if exists
            if (!string.IsNullOrEmpty(product.Value.ImageUrl))
            {
                var blobUri = new Uri(product.Value.ImageUrl);
                var blobName = blobUri.Segments.Last();
                var containerClient = _blobServiceClient.GetBlobContainerClient("product-images");
                var blobClient = containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync();
            }

            // Delete from table storage
            await tableClient.DeleteEntityAsync(partitionKey, rowKey);

            return RedirectToAction(nameof(Index));
        }

        // GET: Home/Details/5
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            var tableClient = _tableServiceClient.GetTableClient("Products");
            var product = await tableClient.GetEntityAsync<Product>(partitionKey, rowKey);
            return View(product.Value);
        }

        private async Task<string> UploadFileToBlob(IFormFile file, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blobClient = containerClient.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            return blobClient.Uri.ToString();
        }

        private async Task<string> UploadFileToAzureFiles(IFormFile file, string shareName)
        {
            var shareClient = new ShareClient(_configuration["AzureStorage:ConnectionString"], shareName);
            await shareClient.CreateIfNotExistsAsync();

            var directoryClient = shareClient.GetRootDirectoryClient();
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fileClient = directoryClient.GetFileClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await fileClient.CreateAsync(stream.Length);
                await fileClient.UploadRangeAsync(new HttpRange(0, stream.Length), stream);
            }

            return fileClient.Uri.ToString();
        }
    }
}