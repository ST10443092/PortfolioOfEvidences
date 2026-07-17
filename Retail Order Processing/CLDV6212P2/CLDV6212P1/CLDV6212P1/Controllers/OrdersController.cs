using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Mvc;
using CLDV6212P1.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text; // Add this at the top with other using statements

public class OrdersController : Controller
{
    private readonly TableClient _ordersTableClient;
    private readonly QueueClient _ordersQueueClient;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IConfiguration configuration,
        ILogger<OrdersController> logger)
    {
        // Initialize configuration and logger
        _logger = logger;

        // Get connection string from configuration
        var connectionString = configuration.GetConnectionString("AzureStorage")
            ?? configuration["AzureStorage:ConnectionString"];

        // Initialize Table Client
        _ordersTableClient = new TableClient(
            connectionString: connectionString,
            tableName: configuration["AzureStorage:OrdersTable"] ?? "Orders");

        // Initialize Queue Client
        _ordersQueueClient = new QueueClient(
            connectionString: connectionString,
            queueName: configuration["AzureStorage:QueueName"] ?? "orders");

        _logger.LogInformation("OrdersController initialized with storage clients");
    }


    // GET: Orders/Index
    public async Task<IActionResult> Index()
    {
        try
        {
            // Ensure table exists
            await _ordersTableClient.CreateIfNotExistsAsync();

            // Query all orders (using partition key filter for efficiency)
            var orders = _ordersTableClient.Query<Order>(filter: $"PartitionKey eq 'Orders'")
                                   .OrderByDescending(o => o.Timestamp)
                                   .ToList();

            return View(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders");
            return View(new List<Order>()); // Return empty list on error
        }
    }

    // GET: Orders/Create
    [HttpGet]
    public IActionResult Create(string productId, string productName, double price,string size)
    {
        var order = new Order
        {
            ProductId = productId,
            ProductName = productName,
            TotalPrice = price,
            Size = size,
            PartitionKey = "Orders",
            RowKey = Guid.NewGuid().ToString()
        };
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Order order)//(microsoft, 2022)
    {
        if (ModelState.IsValid)
        {
            try
            {
                using var httpClient = new HttpClient();

                // Replace with your function URL + key
                var functionUrl = "https://addtable-apaxh9bda7e2azev.eastus-01.azurewebsites.net/api/orders?code=HpO3ESfe3XXXEJdxGb3JY82T52KkMWUagHNRnydmWHYKAzFuAC_TTA==";

                var json = JsonSerializer.Serialize(order);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(functionUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    // Get the returned order (with PartitionKey & RowKey)
                    var result = await response.Content.ReadAsStringAsync();
                    var createdOrder = JsonSerializer.Deserialize<Order>(result);

                    return RedirectToAction("Details", new
                    {
                        partitionKey = createdOrder.PartitionKey,
                        rowKey = createdOrder.RowKey
                    });
                }
                else
                {
                    ModelState.AddModelError("", "Error creating order via Azure Function.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }
        }

        return View(order);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string partitionKey, string rowKey)
    {
        try
        {
            var response = await _ordersTableClient.GetEntityAsync<Order>(
                partitionKey: partitionKey,
                rowKey: rowKey);

            return View(response.Value);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Order not found: {PartitionKey}/{RowKey}", partitionKey, rowKey);
            return NotFound();
        }
    }

    public async Task<IActionResult> Customer()
    {
        try
        {
            // Ensure table exists
            await _ordersTableClient.CreateIfNotExistsAsync();

            // Query all orders (using partition key filter for efficiency)
            var orders = _ordersTableClient.Query<Order>(filter: $"PartitionKey eq 'Orders'")
                                   .OrderByDescending(o => o.Timestamp)
                                   .ToList();

            return View(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders");
            return View(new List<Order>()); // Return empty list on error
        }
    }
}
//Microsoft(2022).Adding a Create Method and Create View. Available at: https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions-1/getting-started-with-mvc/getting-started-with-mvc-part6
//(Accessed: 3 October 2025).