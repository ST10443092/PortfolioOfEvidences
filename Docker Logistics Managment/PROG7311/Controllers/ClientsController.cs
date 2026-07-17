using Microsoft.AspNetCore.Mvc;
using PROG7311.Models;
using System.Net.Http.Json;

namespace PROG7311.Controllers
{
    public class ClientsController : Controller
    {
        private readonly HttpClient _apiClient;

        public ClientsController(IHttpClientFactory httpClientFactory)
        {
            _apiClient = httpClientFactory.CreateClient("BackendApi");
        }

        public async Task<IActionResult> Index(bool fromClient = false)
        {
            ViewBag.FromClient = fromClient;
            var clients = await _apiClient.GetFromJsonAsync<List<Client>>("api/clients") ?? new List<Client>();
            return View(clients);
        }

        public async Task<IActionResult> Details(int? id, bool fromClient = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _apiClient.GetFromJsonAsync<Client>($"api/clients/{id}");
            if (client == null)
            {
                return NotFound();
            }

            ViewBag.FromClient = fromClient;
            return View(client);
        }

        public IActionResult Create(bool fromClient = false)
        {
            ViewBag.FromClient = fromClient;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,Name,ContactDetails,Region")] Client client, bool fromClient = false)
        {
            if (ModelState.IsValid)
            {
                client.Contracts = null;
                client.Users = null;

                var response = await _apiClient.PostAsJsonAsync("api/clients", client);
                if (response.IsSuccessStatusCode)
                {
                    var createdClient = await response.Content.ReadFromJsonAsync<Client>();
                    if (createdClient != null)
                    {
                        await LinkLoggedInClientUserAsync(createdClient.ClientId);
                    }

                    if (fromClient)
                    {
                        return RedirectToAction("Index", "ClientService");
                    }

                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "The client could not be saved through the API.");
            }

            ViewBag.FromClient = fromClient;
            return View(client);
        }

        public async Task<IActionResult> Edit(int? id, bool fromClient = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _apiClient.GetFromJsonAsync<Client>($"api/clients/{id}");
            if (client == null)
            {
                return NotFound();
            }

            ViewBag.FromClient = fromClient;
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClientId,Name,ContactDetails,Region")] Client client, bool fromClient = false)
        {
            if (id != client.ClientId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                client.Contracts = null;
                client.Users = null;

                var response = await _apiClient.PutAsJsonAsync($"api/clients/{id}", client);
                if (response.IsSuccessStatusCode)
                {
                    if (fromClient)
                    {
                        return RedirectToAction("Index", "ClientService");
                    }

                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "The client could not be updated through the API.");
            }

            ViewBag.FromClient = fromClient;
            return View(client);
        }

        public async Task<IActionResult> Delete(int? id, bool fromClient = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _apiClient.GetFromJsonAsync<Client>($"api/clients/{id}");
            if (client == null)
            {
                return NotFound();
            }

            ViewBag.FromClient = fromClient;
            return View(client);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, bool fromClient = false)
        {
            var response = await _apiClient.DeleteAsync($"api/clients/{id}");
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "The client could not be deleted through the API.");
                var client = await _apiClient.GetFromJsonAsync<Client>($"api/clients/{id}");
                ViewBag.FromClient = fromClient;
                return View("Delete", client);
            }

            if (fromClient)
            {
                return RedirectToAction("Index", "ClientService");
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LinkLoggedInClientUserAsync(int clientId)
        {
            var role = HttpContext.Session.GetString("Role");
            var username = HttpContext.Session.GetString("Username");

            if (!string.Equals(role, "Client", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            var users = await _apiClient.GetFromJsonAsync<List<Users>>("api/users") ?? new List<Users>();
            var user = users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                return;
            }

            user.ClientId = clientId;
            user.Client = null;

            var response = await _apiClient.PutAsJsonAsync($"api/users/{user.UsersId}", user);
            if (response.IsSuccessStatusCode)
            {
                HttpContext.Session.SetInt32("ClientId", clientId);
            }
        }
    }
}
