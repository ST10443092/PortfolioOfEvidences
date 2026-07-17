using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ProductFunctions
{
    public class FileFunction
    {
        private readonly string _connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        [Function("UploadContractToFiles")] //(Microsoft, 2024)
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "upload/contract")] HttpRequestData req,
            FunctionContext context)
        {
            var logger = context.GetLogger("UploadContractToFiles");

            try
            {
                // Use the same MultipartReader logic as blob function
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
                    if (contentDisposition != null && contentDisposition.DispositionType.Value.Equals("form-data", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrEmpty(contentDisposition.FileName.Value))
                    {
                        fileName = contentDisposition.FileName.Value;
                        fileStream = new MemoryStream();
                        await section.Body.CopyToAsync(fileStream);
                        fileStream.Position = 0; // important
                        break; // Only one file
                    }
                    section = await reader.ReadNextSectionAsync();
                }

                if (fileStream == null)
                    throw new Exception("No contract uploaded.");

                // Upload to Azure Files
                var shareClient = new ShareClient(_connectionString, "contracts");
                await shareClient.CreateIfNotExistsAsync();
                var directoryClient = shareClient.GetRootDirectoryClient();
                var fileClient = directoryClient.GetFileClient($"{Guid.NewGuid()}_{fileName}");

                await fileClient.CreateAsync(fileStream.Length);
                fileStream.Position = 0; // reset again just in case
                await fileClient.UploadRangeAsync(new Azure.HttpRange(0, fileStream.Length), fileStream);

                // Return JSON just like blob function
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new { contractUrl = fileClient.Uri.ToString() });
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error uploading contract.");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(new { error = ex.Message });
                return response;
            }
        }
    }
}
//Microsoft (2024). Azure Functions File Storage Bindings. Available at: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-file
//(Accessed: 3 October 2025).