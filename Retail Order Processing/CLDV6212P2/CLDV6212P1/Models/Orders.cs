using Azure;
using Azure.Data.Tables;

public class Order : ITableEntity
{
    // Order properties
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }

    public double CustomerNumber { get; set; }

    public string CustomerAdress { get; set; }
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string? Size { get; set; }
    public int Quantity { get; set; }
    public double TotalPrice { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    // Azure Table required properties
    public string PartitionKey { get; set; } = "Orders"; // Static partition key
    public string RowKey { get; set; } = Guid.NewGuid().ToString(); // Unique order ID
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}