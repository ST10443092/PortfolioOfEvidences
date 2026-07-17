using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PROG7311.Models;
using PROG7311.Services;
using System.Net.Http.Json;

namespace PROG7311.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly HttpClient _apiClient;
        private readonly ICurrencyConversionService _currencyConversion;

        public ServiceRequestsController(IHttpClientFactory httpClientFactory, ICurrencyConversionService currencyConversion)
        {
            _apiClient = httpClientFactory.CreateClient("BackendApi");
            _currencyConversion = currencyConversion;
        }

        public async Task<IActionResult> Index()
        {
            var serviceRequests = await _apiClient.GetFromJsonAsync<List<ServiceRequest>>("api/servicerequests") ?? new List<ServiceRequest>();
            return View(serviceRequests);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _apiClient.GetFromJsonAsync<ServiceRequest>($"api/servicerequests/{id}");
            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        public async Task<IActionResult> Create(bool fromClient = false)
        {
            ViewBag.FromClient = fromClient;
            var loggedInClient = await GetLoggedInClientAsync();
            if (loggedInClient != null)
            {
                ViewBag.IsClientUser = true;
                ViewBag.LockedClientId = loggedInClient.ClientId;
                ViewData["ClientId"] = new SelectList(new[] { loggedInClient }, "ClientId", "Name", loggedInClient.ClientId);
            }
            else
            {
                ViewBag.IsClientUser = false;
                ViewData["ClientId"] = new SelectList(await GetClientsAsync(), "ClientId", "Name");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ServiceRequestId,ClientId,Description,Cost,Status")] ServiceRequest serviceRequest, bool fromClient = false)
        {
            var loggedInClient = await GetLoggedInClientAsync();
            if (loggedInClient != null)
            {
                serviceRequest.ClientId = loggedInClient.ClientId;
                ViewBag.IsClientUser = true;
                ViewBag.LockedClientId = loggedInClient.ClientId;
            }
            else
            {
                ViewBag.IsClientUser = false;
            }

            var derivedContract = await GetBestContractForClient(serviceRequest.ClientId);
            if (derivedContract == null)
            {
                ModelState.AddModelError(nameof(ServiceRequest.ClientId), "Selected client does not have a contract.");
            }
            else if (IsBlockedStatus(derivedContract.Status))
            {
                ModelState.AddModelError(nameof(ServiceRequest.ClientId), $"This client cannot submit service requests because their contract status is '{derivedContract.Status}'.");
            }
            else
            {
                serviceRequest.ContractId = derivedContract.ContractId;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await ApplyRegionalCostToZarAsync(serviceRequest, serviceRequest.ClientId, HttpContext.RequestAborted);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(ServiceRequest.Cost), ex.Message);
                }
            }

            if (ModelState.IsValid)
            {
                serviceRequest.Client = null;
                serviceRequest.Contract = null;

                var response = await _apiClient.PostAsJsonAsync("api/servicerequests", ToServiceRequestPayload(serviceRequest));
                if (response.IsSuccessStatusCode)
                {
                    if (fromClient)
                    {
                        return RedirectToAction("Index", "ClientService");
                    }

                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "The service request could not be saved through the API.");
            }

            await PopulateClientSelectList(serviceRequest.ClientId, loggedInClient, fromClient);
            return View(serviceRequest);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _apiClient.GetFromJsonAsync<ServiceRequest>($"api/servicerequests/{id}");
            if (serviceRequest == null)
            {
                return NotFound();
            }

            ViewData["ClientId"] = new SelectList(await GetClientsAsync(), "ClientId", "Name", serviceRequest.ClientId);
            ViewData["ClientRegion"] = serviceRequest.Client?.Region ?? serviceRequest.Contract?.Client?.Region ?? "";
            ViewData["ServiceLevel"] = serviceRequest.Contract?.ServiceLevel ?? "";
            return View(serviceRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ServiceRequestId,ClientId,Description,Cost,Status")] ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.ServiceRequestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existing = await _apiClient.GetFromJsonAsync<ServiceRequest>($"api/servicerequests/{id}");
                if (existing == null)
                {
                    return NotFound();
                }

                var derivedContract = await GetBestContractForClient(serviceRequest.ClientId);
                if (derivedContract == null)
                {
                    ModelState.AddModelError(nameof(ServiceRequest.ClientId), "Selected client does not have a contract.");
                    await PopulateServiceRequestEditLists(serviceRequest, null);
                    return View(serviceRequest);
                }

                if (IsBlockedStatus(derivedContract.Status))
                {
                    ModelState.AddModelError(nameof(ServiceRequest.ClientId), $"This client cannot submit service requests because their contract status is '{derivedContract.Status}'.");
                    await PopulateServiceRequestEditLists(serviceRequest, derivedContract);
                    return View(serviceRequest);
                }

                existing.ClientId = serviceRequest.ClientId;
                existing.ContractId = derivedContract.ContractId;
                existing.Description = serviceRequest.Description;
                existing.Cost = serviceRequest.Cost;
                existing.Status = serviceRequest.Status;

                try
                {
                    await ApplyRegionalCostToZarAsync(existing, existing.ClientId, HttpContext.RequestAborted);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(ServiceRequest.Cost), ex.Message);
                    await PopulateServiceRequestEditLists(serviceRequest, derivedContract);
                    return View(serviceRequest);
                }

                existing.Client = null;
                existing.Contract = null;

                var response = await _apiClient.PutAsJsonAsync($"api/servicerequests/{id}", ToServiceRequestPayload(existing));
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "The service request could not be updated through the API.");
            }

            await PopulateServiceRequestEditLists(serviceRequest, await GetBestContractForClient(serviceRequest.ClientId));
            return View(serviceRequest);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _apiClient.GetFromJsonAsync<ServiceRequest>($"api/servicerequests/{id}");
            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _apiClient.DeleteAsync($"api/servicerequests/{id}");
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "The service request could not be deleted through the API.");
                var serviceRequest = await _apiClient.GetFromJsonAsync<ServiceRequest>($"api/servicerequests/{id}");
                return View("Delete", serviceRequest);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ClientInfo(int clientId)
        {
            var client = await GetClientAsync(clientId);
            var contract = await GetBestContractForClient(clientId);
            var currencyCode = RegionCurrencyMapper.GetCurrencyCodeForRegion(client?.Region);

            return Json(new
            {
                region = client?.Region ?? "",
                serviceLevel = contract?.ServiceLevel ?? "",
                contractStatus = contract?.Status ?? "",
                canSubmitServiceRequest = contract != null && !IsBlockedStatus(contract.Status),
                currencyCode,
                currencySymbol = CurrencyDisplay.Symbol(currencyCode)
            });
        }

        [HttpGet]
        public async Task<IActionResult> CostZarPreview(int clientId, decimal amount, CancellationToken cancellationToken)
        {
            if (clientId <= 0)
            {
                return BadRequest(new { error = "Invalid client." });
            }

            var client = await GetClientAsync(clientId);
            if (client == null)
            {
                return NotFound(new { error = "Client not found." });
            }

            if (amount < 0)
            {
                return BadRequest(new { error = "Amount cannot be negative." });
            }

            var code = RegionCurrencyMapper.GetCurrencyCodeForRegion(client.Region);

            try
            {
                var converted = await _currencyConversion.ConvertToZarAsync(amount, code, cancellationToken);
                return Json(new
                {
                    currencyCode = code,
                    currencySymbol = CurrencyDisplay.Symbol(code),
                    amountRegional = amount,
                    amountZar = converted.AmountInZar,
                    exchangeRateToZar = converted.ExchangeRateZarPerUnit,
                    rateDate = converted.RateDate.ToString("yyyy-MM-dd"),
                    zarFormatted = CurrencyDisplay.FormatZar(converted.AmountInZar),
                    detail = $"1 {code} ≈ R {converted.ExchangeRateZarPerUnit.ToString(System.Globalization.CultureInfo.InvariantCulture)} ZAR (rate date: {converted.RateDate:yyyy-MM-dd})"
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private async Task ApplyRegionalCostToZarAsync(ServiceRequest target, int clientId, CancellationToken cancellationToken)
        {
            var client = await GetClientAsync(clientId)
                ?? throw new InvalidOperationException("Client not found.");

            var fromCode = RegionCurrencyMapper.GetCurrencyCodeForRegion(client.Region);
            var converted = await _currencyConversion.ConvertToZarAsync(target.Cost, fromCode, cancellationToken);

            target.CostCurrencyCode = converted.SourceCurrency;
            target.CostInZar = converted.AmountInZar;
            target.ExchangeRateToZar = converted.ExchangeRateZarPerUnit;
            target.CurrencyRateDate = converted.RateDate;
            target.CurrencyConvertedAtUtc = DateTime.UtcNow;
        }

        private async Task PopulateClientSelectList(int selectedClientId, Client? loggedInClient, bool fromClient)
        {
            ViewBag.FromClient = fromClient;
            if (loggedInClient != null)
            {
                ViewData["ClientId"] = new SelectList(new[] { loggedInClient }, "ClientId", "Name", selectedClientId);
            }
            else
            {
                ViewData["ClientId"] = new SelectList(await GetClientsAsync(), "ClientId", "Name", selectedClientId);
            }
        }

        private async Task PopulateServiceRequestEditLists(ServiceRequest serviceRequest, Contract? contract)
        {
            ViewData["ClientId"] = new SelectList(await GetClientsAsync(), "ClientId", "Name", serviceRequest.ClientId);
            ViewData["ClientRegion"] = (await GetClientAsync(serviceRequest.ClientId))?.Region ?? "";
            ViewData["ServiceLevel"] = contract?.ServiceLevel ?? "";
        }

        private static bool IsBlockedStatus(string? status) =>
            string.Equals(status, "Hold", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "Expired", StringComparison.OrdinalIgnoreCase);

        private async Task<Contract?> GetBestContractForClient(int clientId)
        {
            if (clientId <= 0)
            {
                return null;
            }

            var contracts = await _apiClient.GetFromJsonAsync<List<Contract>>("api/contracts") ?? new List<Contract>();
            var clientContracts = contracts.Where(c => c.ClientId == clientId).ToList();

            return clientContracts
                .Where(c => c.Status == "Active")
                .OrderByDescending(c => c.EndDate)
                .FirstOrDefault()
                ?? clientContracts
                    .OrderByDescending(c => c.EndDate)
                    .FirstOrDefault();
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
                var clientById = await GetClientAsync(sessionClientId.Value);
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

        private async Task<Client?> GetClientAsync(int clientId)
        {
            return await _apiClient.GetFromJsonAsync<Client>($"api/clients/{clientId}");
        }

        private async Task<List<Client>> GetClientsAsync()
        {
            return await _apiClient.GetFromJsonAsync<List<Client>>("api/clients") ?? new List<Client>();
        }

        private static object ToServiceRequestPayload(ServiceRequest serviceRequest)
        {
            return new
            {
                serviceRequest.ServiceRequestId,
                serviceRequest.ClientId,
                serviceRequest.ContractId,
                serviceRequest.Description,
                serviceRequest.Cost,
                serviceRequest.CostCurrencyCode,
                serviceRequest.CostInZar,
                serviceRequest.ExchangeRateToZar,
                serviceRequest.CurrencyRateDate,
                serviceRequest.CurrencyConvertedAtUtc,
                serviceRequest.Status
            };
        }
    }
}
