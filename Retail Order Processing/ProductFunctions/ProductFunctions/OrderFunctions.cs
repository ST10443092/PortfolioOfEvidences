using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using CLDV6212P1.Models;

namespace ProductFunctions
{
    public class OrderFunctions
    {
        private readonly string _connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        [Function("CreateOrder")] //(Microsoft, 2024)
        public async Task<HttpResponseData> CreateOrder(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders")] HttpRequestData req,
            FunctionContext context)
        {
            var logger = context.GetLogger("CreateOrder");

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var order = JsonSerializer.Deserialize<Order>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (order == null)
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteStringAsync("Invalid order data");
                    return badResponse;
                }

                // Table client
                var tableClient = new TableClient(_connectionString, "Orders");
                await tableClient.CreateIfNotExistsAsync();

                // Queue client
                var queueClient = new QueueClient(_connectionString, "orders-queue");
                await queueClient.CreateIfNotExistsAsync();

                // Set table properties
                order.PartitionKey = "Orders";
                order.RowKey = Guid.NewGuid().ToString();

                // Optional: default Size to null if empty
                if (string.IsNullOrWhiteSpace(order.Size))
                    order.Size = null;

                // Save to table
                await tableClient.AddEntityAsync(order);

                // Send minimal message to queue (order ID)
                await queueClient.SendMessageAsync(order.RowKey);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(order);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating order.");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync(ex.Message);
                return errorResponse;
            }
        }
    }
}
//Microsoft (2024). Azure Functions Queue Storage Bindings. Available at: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue
//(Accessed: 3 October 2025).