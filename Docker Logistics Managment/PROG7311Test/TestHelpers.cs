using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PROG7311.Data;
using PROG7311.Models;

namespace PROG7311.Tests
{
    public static class TestHelpers
    {
        public static ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        public static IHttpClientFactory GetHttpClientFactory(ApplicationDbContext context)
        {
            return new TestHttpClientFactory(context);
        }

        private sealed class TestHttpClientFactory : IHttpClientFactory
        {
            private readonly ApplicationDbContext _context;

            public TestHttpClientFactory(ApplicationDbContext context)
            {
                _context = context;
            }

            public HttpClient CreateClient(string name)
            {
                return new HttpClient(new TestApiMessageHandler(_context))
                {
                    BaseAddress = new Uri("https://localhost/")
                };
            }
        }

        private sealed class TestApiMessageHandler : HttpMessageHandler
        {
            private readonly ApplicationDbContext _context;
            private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            public TestApiMessageHandler(ApplicationDbContext context)
            {
                _context = context;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var path = request.RequestUri?.AbsolutePath.Trim('/').ToLowerInvariant() ?? "";
                var query = request.RequestUri?.Query ?? "";

                if (request.Method == HttpMethod.Get && path == "api/clients")
                {
                    return JsonResponse(await _context.Clients.ToListAsync(cancellationToken));
                }

                if (request.Method == HttpMethod.Get && path.StartsWith("api/clients/"))
                {
                    var id = int.Parse(path.Split('/').Last());
                    var client = await _context.Clients.FindAsync([id], cancellationToken);
                    return client == null ? NotFound() : JsonResponse(client);
                }

                if (request.Method == HttpMethod.Get && path == "api/contracts")
                {
                    return JsonResponse(await _context.Contracts.ToListAsync(cancellationToken));
                }

                if (request.Method == HttpMethod.Get && path.StartsWith("api/contracts/"))
                {
                    var id = int.Parse(path.Split('/').Last());
                    var contract = await _context.Contracts.FindAsync([id], cancellationToken);
                    return contract == null ? NotFound() : JsonResponse(contract);
                }

                if (request.Method == HttpMethod.Post && path == "api/contracts")
                {
                    var contract = await ReadJsonAsync<Contract>(request, cancellationToken);
                    _context.Contracts.Add(contract!);
                    await _context.SaveChangesAsync(cancellationToken);
                    return JsonResponse(contract!, HttpStatusCode.Created);
                }

                if (request.Method == HttpMethod.Get && path == "api/users")
                {
                    return JsonResponse(await _context.Users.ToListAsync(cancellationToken));
                }

                if (request.Method == HttpMethod.Get && path == "api/users/by-credentials")
                {
                    var values = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(query);
                    var username = values["username"].ToString();
                    var password = values["password"].ToString();
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password, cancellationToken);
                    return user == null ? NotFound() : JsonResponse(user);
                }

                if (request.Method == HttpMethod.Post && path == "api/servicerequests")
                {
                    var serviceRequest = await ReadJsonAsync<ServiceRequest>(request, cancellationToken);
                    _context.ServiceRequests.Add(serviceRequest!);
                    await _context.SaveChangesAsync(cancellationToken);
                    return JsonResponse(serviceRequest!, HttpStatusCode.Created);
                }

                return NotFound();
            }

            private async Task<T?> ReadJsonAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var json = await request.Content!.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }

            private HttpResponseMessage JsonResponse<T>(T value, HttpStatusCode statusCode = HttpStatusCode.OK)
            {
                return new HttpResponseMessage(statusCode)
                {
                    Content = JsonContent.Create(value, options: _jsonOptions)
                };
            }

            private static HttpResponseMessage NotFound()
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }
    }
}
