using Microsoft.AspNetCore.Mvc;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using Azure;

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
    public async Task<IActionResult> Create(Order order)
    {
        if (ModelState.IsValid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(order.Size))
                {
                    order.Size = null;
                }
                // Ensure resources exist
                await _ordersTableClient.CreateIfNotExistsAsync();
                await _ordersQueueClient.CreateIfNotExistsAsync();

                // Save order to table storage
                await _ordersTableClient.AddEntityAsync(order);

                // Add minimal data to queue (just order ID)
                await _ordersQueueClient.SendMessageAsync(order.RowKey);

                return RedirectToAction("Details", new
                {
                    partitionKey = order.PartitionKey,
                    rowKey = order.RowKey
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                ModelState.AddModelError("", "Error processing your order. Please try again.");
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