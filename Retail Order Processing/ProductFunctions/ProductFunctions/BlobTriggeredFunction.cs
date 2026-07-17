using Azure.Storage.Blobs;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;  

namespace ProductFunctions
{
    public class BlobFunction
    {
        private readonly string _connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        [Function("UploadImageToBlob")]//(Microsoft, 2024)
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "upload/image")] HttpRequestData req,
            FunctionContext context)
        {
            var logger = context.GetLogger("UploadImageToBlob");

            try
            {
                // Use MultipartReader to parse request body
                var contentType = req.Headers.GetValues("Content-Type").FirstOrDefault();
                var boundary = Microsoft.Net.Http.Headers.HeaderUtilities.RemoveQuotes(
                    Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse(contentType).Boundary
                ).Value;

                var reader = new Microsoft.AspNetCore.WebUtilities.MultipartReader(boundary, req.Body);
                var section = await reader.ReadNextSectionAsync();

                Stream fileStream = null;
                string fileName = null;

                while (section != null)
                {
                    var contentDisposition = section.GetContentDispositionHeader();
                    if (contentDisposition != null && contentDisposition.DispositionType.Value.Equals("form-data", StringComparison.OrdinalIgnoreCase) && !Microsoft.Extensions.Primitives.StringSegment.IsNullOrEmpty(contentDisposition.FileName))
                    {
                        fileName = contentDisposition.FileName.Value;
                        fileStream = new MemoryStream();
                        await section.Body.CopyToAsync(fileStream);
                        fileStream.Position = 0;
                        break; // Only one file
                    }
                    section = await reader.ReadNextSectionAsync();
                }

                if (fileStream == null)
                    throw new Exception("No file uploaded.");

                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient("product-images");
                await containerClient.CreateIfNotExistsAsync();

                var blobClient = containerClient.GetBlobClient($"{Guid.NewGuid()}_{fileName}");
                await blobClient.UploadAsync(fileStream, true);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new { imageUrl = blobClient.Uri.ToString() });
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error uploading image to blob.");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync(ex.Message);
                return response;
            }
        }
    }
}
//Microsoft (2024). Azure Functions Blob Storage Bindings. Available at: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-blob
//(Accessed: 3 October 2025).