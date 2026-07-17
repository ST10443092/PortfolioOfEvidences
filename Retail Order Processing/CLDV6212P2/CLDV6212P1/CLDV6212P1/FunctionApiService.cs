using CLDV6212P1.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;

public class FunctionApiService
{
    private readonly HttpClient _httpClient;

    // Base URL for your Function App (without query string)
    private readonly string _baseUrl = "https://addtable-apaxh9bda7e2azev.eastus-01.azurewebsites.net/api";

    public FunctionApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Upload image to Blob function
    public async Task<string> UploadImageAsync(IFormFile imageFile)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StreamContent(imageFile.OpenReadStream()), "ImageFile", imageFile.FileName);

        var response = await _httpClient.PostAsync($"{_baseUrl}/upload/image", form);
        response.EnsureSuccessStatusCode();

        var resultJson = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(resultJson).RootElement.GetProperty("imageUrl").GetString()!;
    }

    // Upload contract to Azure Files
    public async Task<string> UploadContractAsync(IFormFile contractFile)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StreamContent(contractFile.OpenReadStream()), "ContractFile", contractFile.FileName);

        var response = await _httpClient.PostAsync($"{_baseUrl}/upload/contract", form);
        response.EnsureSuccessStatusCode();

        var resultJson = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(resultJson).RootElement.GetProperty("contractUrl").GetString()!;
    }

    // Save product to Table Storage
    public async Task<bool> CreateProductAsync(Product product)
    {
        // Include your function key in the query string
        var response = await _httpClient.PostAsJsonAsync(
            $"{_baseUrl}/Products?code=5jO_T0-QUnQIxqr5JyJJ52E3d7lyAX-fEjOknNdp-mObAzFu4QLgPQ==",
            product
        );
        return response.IsSuccessStatusCode;
    }

    // Optional: fetch all products
    public async Task<List<Product>> GetProductsAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/Products");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Product>>(result)!;
    }
}
