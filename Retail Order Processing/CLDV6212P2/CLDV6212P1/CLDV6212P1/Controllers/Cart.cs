using CLDV6212P1.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Azure.Data.Tables;

namespace CLDV6212P1.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "Cart";
        private readonly TableClient _orderTable;
        private readonly ILogger<CartController> _logger;


        public CartController(IConfiguration configuration, ILogger<CartController> logger)
        {
            _logger = logger;

            // ✅ Get connection string from AzureStorage section
            var connectionString = configuration["AzureStorage:ConnectionString"];

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("AzureStorage connection string is not configured.");
                throw new InvalidOperationException("AzureStorage connection string is not configured.");
            }

            // ✅ Initialize TableClient for orders
            _orderTable = new TableClient(
                connectionString: connectionString,
                tableName: "Order"); // your Azure table name
            
            // Ensure table exists
            _orderTable.CreateIfNotExists();

            _logger.LogInformation("CartController initialized with Azure Table Storage connection.");
        }

        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(json))
                return new List<CartItem>();
            
            try
            {
                return JsonConvert.DeserializeObject<List<CartItem>>(json) ?? new List<CartItem>();
            }
            catch
            {
                return new List<CartItem>();
            }
        }

        private void SaveCart(List<CartItem> cart)
        {
            var json = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString(CartSessionKey, json);
        }

        // ✅ Add product with size
        public IActionResult Add(string productId, string productName, double unitPrice, string imageUrl, string size)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(productId) || string.IsNullOrEmpty(size))
            {
                TempData["Error"] = "Product ID and size are required.";
                return RedirectToAction("Index", "Home");
            }

            var cart = GetCart();
            var existing = cart.FirstOrDefault(c => c.ProductId == productId && c.Size == size);

            if (existing != null)
                existing.Quantity++;
            else
                cart.Add(new CartItem
                {
                    ProductId = productId,
                    ProductName = productName ?? "Unknown Product",
                    UnitPrice = unitPrice,
                    ImageUrl = imageUrl ?? "",
                    Size = size
                });

            SaveCart(cart);
            TempData["Message"] = $"{productName} (Size: {size}) added to cart!";
            return RedirectToAction("Index", "Home");
        }

        // ✅ View cart
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // ✅ Remove item
        public IActionResult Remove(string productId, string size)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == productId && c.Size == size);
            if (item != null) cart.Remove(item);
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        // ✅ Clear cart
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CartSessionKey);
            TempData["Message"] = "Cart cleared successfully!";
            return RedirectToAction("Index");
        }

        // ✅ Update quantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(string productId, string size, int quantity)
        {
            if (quantity <= 0)
            {
                return Remove(productId, size);
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == productId && c.Size == size);
            if (item != null)
            {
                item.Quantity = quantity;
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        // ✅ Checkout page
        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (!cart.Any())
                return RedirectToAction("Index");

            return View(new CheckoutViewModel());
        }

        // ✅ Process checkout
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cart = GetCart();

            if (!ModelState.IsValid)
                return View(model);

            foreach (var item in cart)
            {
                var order = new Order
                {
                    CustomerEmail = model.CustomerEmail,
                    CustomerAdress = model.DeliveryAddress,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Size = item.Size,
                    Quantity = item.Quantity,
                    TotalPrice = item.TotalPrice,
                    RowKey = Guid.NewGuid().ToString() // Generate unique RowKey for each order item
                };

                await _orderTable.AddEntityAsync(order);
            }

            // Clear the cart after saving
            HttpContext.Session.Remove(CartSessionKey);

            TempData["Message"] = "Your order has been placed successfully!";
            return RedirectToAction("Index", "Home");
        }
    }
}
