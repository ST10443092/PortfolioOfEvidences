using Microsoft.AspNetCore.Mvc;
using PROG7311.Models;
using System.Net.Http.Json;

namespace PROG7311.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpClient _apiClient;

        public LoginController(IHttpClientFactory httpClientFactory)
        {
            _apiClient = httpClientFactory.CreateClient("BackendApi");
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string username, string password)
        {
            var response = await _apiClient.GetAsync(
                $"api/users/by-credentials?username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                ViewBag.Error = "Invalid username or password";
                return View();
            }

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Login could not be completed through the API.";
                return View();
            }

            var user = await response.Content.ReadFromJsonAsync<Users>();
            if (user == null)
            {
                ViewBag.Error = "Invalid username or password";
                return View();
            }

            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.Remove("ClientId");

            if (string.Equals(user.Role, "Client", StringComparison.OrdinalIgnoreCase))
            {
                var resolvedClientId = user.ClientId;
                if (!resolvedClientId.HasValue)
                {
                    var clients = await _apiClient.GetFromJsonAsync<List<Client>>("api/clients") ?? new List<Client>();
                    var existingClient = clients.FirstOrDefault(c => c.Name == user.Username);
                    if (existingClient != null)
                    {
                        resolvedClientId = existingClient.ClientId;
                        user.ClientId = existingClient.ClientId;
                        user.Client = null;
                        await _apiClient.PutAsJsonAsync($"api/users/{user.UsersId}", user);
                    }
                }

                if (resolvedClientId.HasValue)
                {
                    HttpContext.Session.SetInt32("ClientId", resolvedClientId.Value);
                }
            }

            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "AdminView");
            }

            if (user.Role == "Client")
            {
                return RedirectToAction("Index", "ClientService");
            }

            ViewBag.Error = "User role is invalid";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
