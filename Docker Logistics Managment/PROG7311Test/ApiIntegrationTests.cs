using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace PROG7311.Tests
{
    public class ApiIntegrationTests
    {
        private readonly HttpClient _client;

        public ApiIntegrationTests()
        {
            var baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL")
                ?? "http://localhost:5097/";

            _client = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        [Fact]
        public async Task GetContracts_ShouldReturnOkAndJson()
        {
            var response = await _client.GetAsync("api/contracts");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var contracts = await response.Content.ReadFromJsonAsync<List<object>>();

            Assert.NotNull(contracts);
        }

        [Fact]
        public async Task GetClients_ShouldReturnOkAndJson()
        {
            var response = await _client.GetAsync("api/clients");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var clients = await response.Content.ReadFromJsonAsync<List<object>>();

            Assert.NotNull(clients);
        }

        [Fact]
        public async Task GetUsers_ShouldReturnOkAndJson()
        {
            var response = await _client.GetAsync("api/users");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var users = await response.Content.ReadFromJsonAsync<List<object>>();

            Assert.NotNull(users);
        }
    }
}