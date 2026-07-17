using Microsoft.AspNetCore.Mvc;
using PROG7311.Models;
using System.Net.Http.Json;

namespace PROG7311.Controllers
{
    public class UsersController : Controller
    {
        private readonly HttpClient _apiClient;

        public UsersController(IHttpClientFactory httpClientFactory)
        {
            _apiClient = httpClientFactory.CreateClient("BackendApi");
        }

        public async Task<IActionResult> Index()
        {
            var users = await _apiClient.GetFromJsonAsync<List<Users>>("api/users") ?? new List<Users>();
            return View(users);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _apiClient.GetFromJsonAsync<Users>($"api/users/{id}");
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UsersId,Username,Password,Role")] Users users)
        {
            users.Role = "Client";
            users.Client = null;

            if (ModelState.IsValid)
            {
                var response = await _apiClient.PostAsJsonAsync("api/users", users);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Login");
                }

                ModelState.AddModelError(string.Empty, "The user could not be saved through the API.");
            }

            return View(users);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _apiClient.GetFromJsonAsync<Users>($"api/users/{id}");
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UsersId,Username,Password,Role,ClientId")] Users users)
        {
            if (id != users.UsersId)
            {
                return NotFound();
            }

            users.Client = null;

            if (ModelState.IsValid)
            {
                var response = await _apiClient.PutAsJsonAsync($"api/users/{id}", users);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "The user could not be updated through the API.");
            }

            return View(users);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _apiClient.GetFromJsonAsync<Users>($"api/users/{id}");
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _apiClient.DeleteAsync($"api/users/{id}");
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "The user could not be deleted through the API.");
                var user = await _apiClient.GetFromJsonAsync<Users>($"api/users/{id}");
                return View("Delete", user);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
