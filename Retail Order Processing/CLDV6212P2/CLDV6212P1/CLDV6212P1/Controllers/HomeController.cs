using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Mvc;
using CLDV6212P1.Models;
using CLDV6212P1.Data;
using CLDV6212P1.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace CLDV6212P1.Controllers
{
    public class HomeController : Controller
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly TableServiceClient _tableServiceClient;
        private readonly TableClient _tableClient;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;

        // 🔗 Replace this with your Function App URL
        private readonly string _functionBaseUrl = "https://addtable-apaxh9bda7e2azev.eastus-01.azurewebsites.net";

        public HomeController(IConfiguration configuration, IHttpClientFactory httpClientFactory, ApplicationDbContext context)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _context = context;

            // Old direct Azure clients (still here so nothing breaks)
            _blobServiceClient = new BlobServiceClient(
                _configuration["AzureStorage:ConnectionString"]
            );

            _tableServiceClient = new TableServiceClient(
                _configuration["AzureStorage:ConnectionString"]
            );

            _tableClient = _tableServiceClient.GetTableClient("Products");
            _tableClient.CreateIfNotExists();
        }

        // GET: Home/Index
        public async Task<IActionResult> Index()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction(nameof(Login));
            }

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
        // GET: Home/Login
        public IActionResult Login()
        {
            // If already logged in, redirect to home
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        // POST: Home/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View();
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);
                
                if (user == null || !PasswordHelper.VerifyPassword(Password, user.PasswordHash))
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                    return View();
                }

                // Store user info in session
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("UserName", $"{user.Name} {user.Surname}");
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", user.Role);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View();
            }
        }

        // GET: Home/Register
        public IActionResult Register()
        {
            // If already logged in, redirect to home
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        // POST: Home/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user, string Password)
        {
            if (string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError("Password", "Password is required.");
                return View(user);
            }

            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return View(user);
            }

            try
            {
                // Hash the password
                user.PasswordHash = Password;
                user.DateCreated = DateTime.Now;
                user.Role = "User"; // Default role

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Auto-login after registration
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("UserName", $"{user.Name} {user.Surname}");
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", user.Role);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View(user);
            }
        }

        // GET: Home/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile, IFormFile contractFile)//Microsoft (2022)
        {
            if (contractFile == null || contractFile.Length == 0)
            {
                ModelState.AddModelError("ContractFile", "Supplier contract is required");
                return View(product);
            }

            try
            {
                using var httpClient = new HttpClient();

                // 1️⃣ Upload image via Blob function
                if (imageFile != null && imageFile.Length > 0)
                {
                    using var imgContent = new MultipartFormDataContent();
                    imgContent.Add(new StreamContent(imageFile.OpenReadStream()), "ImageFile", imageFile.FileName);

                    var imgResponse = await httpClient.PostAsync(
                        "https://addtable-apaxh9bda7e2azev.eastus-01.azurewebsites.net/api/upload/image?code=DV7Rv-058c5ecFAotFxOSWaEU_3Jyanagme2h-vdMik0AzFuowOVYA==",
                        imgContent
                    );

                    if (!imgResponse.IsSuccessStatusCode)
                    {
                        ModelState.AddModelError("", "Failed to upload product image.");
                        return View(product);
                    }

                    var imgResult = await imgResponse.Content.ReadAsStringAsync();
                    product.ImageUrl = System.Text.Json.JsonDocument.Parse(imgResult)
                                        .RootElement.GetProperty("imageUrl")
                                        .GetString();
                }

                // 2️⃣ Upload contract via Azure Function
                if (contractFile != null && contractFile.Length > 0)
                {
                    using var pdfContent = new MultipartFormDataContent();
                    pdfContent.Add(new StreamContent(contractFile.OpenReadStream()), "ContractFile", contractFile.FileName);

                    var contractFunctionUrl = "https://addtable-apaxh9bda7e2azev.eastus-01.azurewebsites.net/api/upload/contract?code=6cYaf8xuj7PBoQKwOvzN-wJgij2rQr_o0eReE2RDlBybAzFuIsKICg==";
                    var contractResponse = await httpClient.PostAsync(contractFunctionUrl, pdfContent);

                    if (contractResponse.IsSuccessStatusCode)
                    {
                        // The function returns JSON with contractUrl property
                        var jsonResult = await contractResponse.Content.ReadAsStringAsync();
                        var contractData = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonResult);
                        product.ContractUrl = contractData?["contractUrl"];
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to upload contract.");
                        return View(product);
                    }
                }

                // 3️⃣ Save product to Azure Table Storage via Table Function
                var tableResponse = await httpClient.PostAsJsonAsync(
                    "https://addtable-apaxh9bda7e2azev.eastus-01.azurewebsites.net/api/products?code=KH9nwMayXAKYqcJhtqaGZqsfplMgU8Hghx_fGmWoxK0fAzFu4KawOg==",
                    product
                );

                if (!tableResponse.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Failed to save product to the database.");
                    return View(product);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(product);
            }
        }




        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var tableClient = _tableServiceClient.GetTableClient("Products");
            var product = await tableClient.GetEntityAsync<Product>(partitionKey, rowKey); //Microsoft(2023)
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
        public async Task<IActionResult> Details(string partitionKey, string rowKey)//Microsoft (2025)  
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

        private async Task<string> UploadFileToAzureFiles(IFormFile file, string shareName)//Microsoft (2025)
        {
            var shareClient = new ShareClient(_configuration["AzureStorage:ConnectionString"], shareName);
            await shareClient.CreateIfNotExistsAsync();

            var directoryClient = shareClient.GetRootDirectoryClient();
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";//C-Sharp Corner (2025)
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
//Microsoft(2022).Adding a Create Method and Create View. Available at: https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions-1/getting-started-with-mvc/getting-started-with-mvc-part6
//(Accessed: 3 October 2025).

