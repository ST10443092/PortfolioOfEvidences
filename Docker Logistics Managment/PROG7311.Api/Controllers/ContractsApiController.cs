using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG7311.Api.Data;
using PROG7311.Api.Models;

namespace PROG7311.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContractsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/contracts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contract>>> GetContracts(
            DateTime? startDateFrom,
            DateTime? endDateTo,
            string? status)
        {
            var contractsQuery = _context.Contracts
                .Include(c => c.Client)
                .AsQueryable();

            if (startDateFrom.HasValue)
            {
                contractsQuery = contractsQuery.Where(c => c.StartDate >= startDateFrom.Value);
            }

            if (endDateTo.HasValue)
            {
                contractsQuery = contractsQuery.Where(c => c.EndDate <= endDateTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                contractsQuery = contractsQuery.Where(c => c.Status == status);
            }

            return await contractsQuery
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();
        }

        // GET: api/contracts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Contract>> GetContract(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.ContractId == id);

            if (contract == null)
            {
                return NotFound();
            }

            return contract;
        }

        // POST: api/contracts
        [HttpPost]
        public async Task<ActionResult<Contract>> CreateContract(ContractRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Status) || string.IsNullOrWhiteSpace(request.ServiceLevel))
            {
                return BadRequest("Status and service level are required.");
            }

            var contract = new Contract
            {
                ClientId = request.ClientId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status,
                ServiceLevel = request.ServiceLevel,
                SignedAgreement = request.SignedAgreement
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetContract), new { id = contract.ContractId }, contract);
        }

        // PUT: api/contracts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContract(int id, ContractRequest request)
        {
            if (id != request.ContractId)
            {
                return BadRequest();
            }

            var existingContract = await _context.Contracts.FindAsync(id);
            if (existingContract == null)
            {
                return NotFound();
            }

            existingContract.ClientId = request.ClientId;
            existingContract.StartDate = request.StartDate;
            existingContract.EndDate = request.EndDate;
            existingContract.Status = string.IsNullOrWhiteSpace(request.Status)
                ? existingContract.Status
                : request.Status;
            existingContract.ServiceLevel = string.IsNullOrWhiteSpace(request.ServiceLevel)
                ? existingContract.ServiceLevel
                : request.ServiceLevel;

            if (request.SignedAgreement != null && request.SignedAgreement.Length > 0)
            {
                existingContract.SignedAgreement = request.SignedAgreement;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
        }

        // PATCH: api/contracts/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateContractStatus(
            int id,
            [FromBody] ContractStatusUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest("Status is required.");
            }

            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null)
            {
                return NotFound();
            }

            contract.Status = request.Status;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/contracts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null)
            {
                return NotFound();
            }

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.ContractId == id);
        }
    }

    public class ContractStatusUpdateRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class ContractRequest
    {
        public int ContractId { get; set; }
        public int ClientId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Status { get; set; }
        public string? ServiceLevel { get; set; }
        public byte[]? SignedAgreement { get; set; }
    }
}
