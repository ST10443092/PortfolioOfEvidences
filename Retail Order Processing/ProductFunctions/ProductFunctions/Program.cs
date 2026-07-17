using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Data.Tables;

var builder = FunctionsApplication.CreateBuilder(args);

// ✅ Enable HTTP pipeline for isolated worker
builder.ConfigureFunctionsWebApplication();

// ✅ Add Application Insights for monitoring
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// ✅ Register Azure Storage services with DI
var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
    ?? throw new InvalidOperationException("AzureWebJobsStorage is not set.");

// Blob service client
builder.Services.AddSingleton(new BlobServiceClient(connectionString));

// File share client (root, you can create per share in code)
builder.Services.AddSingleton(new ShareServiceClient(connectionString));

// Table service client
builder.Services.AddSingleton(new TableServiceClient(connectionString));



builder.Build().Run();
