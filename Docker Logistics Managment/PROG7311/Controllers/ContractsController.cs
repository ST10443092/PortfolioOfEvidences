using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PROG7311.Models;
using System.Net.Http.Json;

namespace PROG7311.Controllers
{
    public class ContractsController : Controller
    {
        private readonly HttpClient _apiClient;

        public ContractsController(IHttpClientFactory httpClientFactory)
        {
            _apiClient = httpClientFactory.CreateClient("BackendApi");
        }

        public async Task<IActionResult> Index(DateTime? startDateFrom, DateTime? endDateTo, string? status)
        {
            var query = new List<string>();

            if (startDateFrom.HasValue)
            {
                query.Add($"startDateFrom={startDateFrom.Value:yyyy-MM-dd}");
            }

            if (endDateTo.HasValue)
            {
                query.Add($"endDateTo={endDateTo.Value:yyyy-MM-dd}");
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query.Add($"status={Uri.EscapeDataString(status)}");
            }

            var url = "api/contracts";
            if (query.Count > 0)
            {
                url += "?" + string.Join("&", query);
            }

            var contracts = await _apiClient.GetFromJsonAsync<List<Contract>>(url) ?? new List<Contract>();

            ViewBag.StartDateFrom = startDateFrom?.ToString("yyyy-MM-dd");
            ViewBag.EndDateTo = endDateTo?.ToString("yyyy-MM-dd");
            ViewBag.SelectedStatus = status;
            ViewBag.StatusOptions = contracts
                .Where(c => !string.IsNullOrWhiteSpace(c.Status))
                .Select(c => c.Status!)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            return View(contracts);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _apiClient.GetFromJsonAsync<Contract>($"api/contracts/{id}");
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var contract = await _apiClient.GetFromJsonAsync<Contract>($"api/contracts/{id}");
            if (contract == null)
            {
                return NotFound();
            }

            if (contract.SignedAgreement == null || contract.SignedAgreement.Length == 0)
            {
                return NotFound();
            }

            var downloadName = $"contract_{contract.ContractId}.pdf";
            return File(contract.SignedAgreement, "application/pdf", downloadName);
        }

        public async Task<IActionResult> Create(bool fromClient = false)
        {
            ViewBag.FromClient = fromClient;
            var loggedInClient = await GetLoggedInClientAsync();
            if (loggedInClient != null)
            {
                ViewBag.IsClientUser = true;
                ViewBag.LockedClientId = loggedInClient.ClientId;
                ViewData["ClientId"] = new SelectList(new[] { loggedInClient }, "ClientId", "ContactDetails", loggedInClient.ClientId);
            }
            else
            {
                ViewBag.IsClientUser = false;
                ViewData["ClientId"] = new SelectList(await GetClientsAsync(), "ClientId", "ContactDetails");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContractId,ClientId,StartDate,EndDate,Status,ServiceLevel,PdfFile")] Contract contract, bool fromClient = false)
        {
            var loggedInClient = await GetLoggedInClientAsync();
            if (loggedInClient != null)
            {
                contract.ClientId = loggedInClient.ClientId;
                ViewBag.IsClientUser = true;
                ViewBag.LockedClientId = loggedInClient.ClientId;
            }
            else
            {
                ViewBag.IsClientUser = false;
            }

            ValidatePdf(contract.PdfFile, required: true);

            if (ModelState.IsValid)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await contract.PdfFile!.CopyToAsync(memoryStream);
                    contract.SignedAgreement = memoryStream.ToArray();
                }

                contract.Client = null;
                contract.ServiceRequests = null;
                contract.PdfFile = null;

                var response = await _apiClient.PostAsJsonAsync("api/contracts", ToContractPayload(contract));
                if (response.IsSuccessStatusCode)
                {
                    if (fromClient)
                    {
                        return RedirectToAction("Index", "ClientService");
                    }

                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, await BuildApiErrorMessage(response, "saved"));
            }

            await PopulateClientSelectList(contract.ClientId, loggedInClient, fromClient);
            return View(contract);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _apiClient.GetFromJsonAsync<Contract>($"api/contracts/{id}");
            if (contract == null)
            {
                return NotFound();
            }

            ViewData["ClientId"] = new SelectList(await GetClientsAsync(), "ClientId", "ContactDetails", contract.ClientId);
            return View(contract);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ContractId,ClientId,StartDate,EndDate,Status,ServiceLevel,PdfFile")] Contract contract)
        {
            if (id != contract.ContractId)
            {
                return NotFound();
            }

            ValidatePdf(contract.PdfFile, required: false);

            if (ModelState.IsValid)
            {
                var existing = await _apiClient.GetFromJsonAsync<Contract>($"api/contracts/{id}");
                if (existing == null)
                {
                    return NotFound();
                }

                existing.ClientId = contract.ClientId;
                existing.StartDate = contract.StartDate;
                existing.EndDate = contract.EndDate;
                existing.Status = string.IsNullOrWhiteSpace(contract.Status) ? existing.Status : contract.Status;
                existing.ServiceLevel = string.IsNullOrWhiteSpace(contract.ServiceLevel) ? existing.ServiceLevel : contract.ServiceLevel;

                if (contract.PdfFile != null && contract.PdfFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await contract.PdfFile.CopyToAsync(memoryStream);
                        existing.SignedAgreement = memoryStream.ToArray();
                    }
                }

                existing.Client = null;
                existing.ServiceRequests = null;
                existing.PdfFile = null;

                var response = await _apiClient.PutAsJsonAsync($"api/contracts/{id}", ToContractPayload(existing));
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, await BuildApiErrorMessage(response, "updated"));
            }

            var current = await _apiClient.GetFromJsonAsync<Contract>($"api/contracts/{id}");
            contract.SignedAgreement = current?.SignedAgreement;

            ViewData["ClientId"] = new SelectList(await GetClientsAsync(), "ClientId", "ContactDetails", contract.ClientId);
            return View(contract);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _apiClient.GetFromJsonAsync<Contract>($"api/contracts/{id}");
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _apiClient.DeleteAsync($"api/contracts/{id}");
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "The contract could not be deleted through the API.");
                var contract = await _apiClient.GetFromJsonAsync<Contract>($"api/contracts/{id}");
                return View("Delete", contract);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<Client>> GetClientsAsync()
        {
            return await _apiClient.GetFromJsonAsync<List<Client>>("api/clients") ?? new List<Client>();
        }

        private async Task PopulateClientSelectList(int selectedClientId, Client? loggedInClient, bool fromClient)
        {
            ViewBag.FromClient = fromClient;
            if (loggedInClient != null)
            {
                ViewData["ClientId"] = new SelectList(new[] { loggedInClient }, "ClientId", "ContactDetails", selectedClientId);
            }
            else
            {
                ViewData["ClientId"] = new SelectList(await GetClientsAsync(), "ClientId", "ContactDetails", selectedClientId);
            }
        }

        private async Task<Client?> GetLoggedInClientAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (!string.Equals(role, "Client", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var sessionClientId = HttpContext.Session.GetInt32("ClientId");
            if (sessionClientId.HasValue)
            {
                var clientById = await _apiClient.GetFromJsonAsync<Client>($"api/clients/{sessionClientId.Value}");
                if (clientById != null)
                {
                    return clientById;
                }
            }

            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            var clients = await GetClientsAsync();
            return clients.FirstOrDefault(c => c.Name == username);
        }

        private void ValidatePdf(IFormFile? pdfFile, bool required)
        {
            if (pdfFile == null || pdfFile.Length == 0)
            {
                if (required)
                {
                    ModelState.AddModelError("PdfFile", "A signed agreement PDF is required.");
                }

                return;
            }

            if (!Path.GetExtension(pdfFile.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("PdfFile", "Only PDF files are allowed.");
            }

            if (!string.Equals(pdfFile.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("PdfFile", "Invalid file type. Must be a PDF.");
            }

            const long maxBytes = 10 * 1024 * 1024;
            if (pdfFile.Length > maxBytes)
            {
                ModelState.AddModelError("PdfFile", "PDF is too large (max 10MB).");
            }
        }

        private static object ToContractPayload(Contract contract)
        {
            return new
            {
                contract.ContractId,
                contract.ClientId,
                contract.StartDate,
                contract.EndDate,
                contract.Status,
                contract.ServiceLevel,
                contract.SignedAgreement
            };
        }

        private static async Task<string> BuildApiErrorMessage(HttpResponseMessage response, string action)
        {
            var details = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(details))
            {
                details = response.ReasonPhrase ?? "No details returned.";
            }

            return $"The contract could not be {action} through the API. API returned {(int)response.StatusCode} {response.StatusCode}: {details}";
        }
    }
}
