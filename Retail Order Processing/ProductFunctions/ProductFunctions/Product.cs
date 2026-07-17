using Azure.Data.Tables;
using Azure; // For ETag
using Azure.Data.Tables; // For ITableEntity
using System.ComponentModel.DataAnnotations;

namespace CLDV6212P1.Models
{
    public class Product : ITableEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string ImageUrl { get; set; }

        public string ContractUrl { get; set; }
        public int StockQuantity { get; set; }

        // Required by ITableEntity
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
