using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using CLDV6212P1.Models;

namespace ProductFunctions
{
    public class TableFunction
    {
        private readonly ILogger<TableFunction> _logger;
        private readonly string _connectionString;

        public TableFunction(ILogger<TableFunction> logger)//(Microsoft, 2024)
        {
            _logger = logger;
            _connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                ?? throw new InvalidOperationException("AzureWebJobsStorage environment variable is not set.");
        }

        [Function("SaveProductToTable")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "products")] HttpRequestData req)
        {
            var response = req.CreateResponse();

            try
            {
                using var reader = new StreamReader(req.Body);
                var requestBody = await reader.ReadToEndAsync();

                var product = JsonSerializer.Deserialize<Product>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (product == null)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    await response.WriteStringAsync("Invalid product data.");
                    return response;
                }

                product.PartitionKey = "Products";
                product.RowKey = Guid.NewGuid().ToString();
                product.Timestamp = DateTimeOffset.UtcNow;

                var tableClient = new TableClient(_connectionString, "Products");
                await tableClient.CreateIfNotExistsAsync();
                await tableClient.AddEntityAsync(product);

                response.StatusCode = HttpStatusCode.OK;
                await response.WriteAsJsonAsync(new { success = true, id = product.RowKey });
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving product to table.");
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync(ex.Message);
                return response;
            }
        }
    }
}
//Microsoft (2024). Azure Functions Table Storage Bindings. Available at: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-table
//(Accessed: 3 October 2025).