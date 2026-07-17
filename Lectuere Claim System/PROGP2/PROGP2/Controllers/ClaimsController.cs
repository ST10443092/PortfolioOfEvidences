using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO; // for Path and MemoryStream
using Microsoft.AspNetCore.Http; // for IFormFile
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PROGP2.Data;
using PROGP2.Models;

namespace PROGP2.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClaimsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Claims
        public async Task<IActionResult> Index()
        {
            // Filter by session role and user when Employee
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetString("UserID");

            IQueryable<Claim> query = _context.Claims.Include(c => c.User);

            if (string.Equals(role, "Employee", StringComparison.OrdinalIgnoreCase) && int.TryParse(userId, out var empId))
            {
                query = query.Where(c => c.UserID == empId);
            }

            return View(await query.ToListAsync());
        }

        // GET: Claims/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claim = await _context.Claims
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (claim == null)
            {
                return NotFound();
            }

            return View(claim);
        }

        // GET: Claims/Create
        public IActionResult Create()
        {
            var userIdString = HttpContext.Session.GetString("UserID");
            if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out var userId))
            {
                ViewData["UserID"] = userId;
            }
            return View();
        }

        // POST: Claims/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Hours,Rate,Status")] Claim claim, IFormFile pdfFile)
        {
            var userIdString = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                TempData["Error"] = "You must be logged in to create a claim.";
                return RedirectToAction("Login", "Home");
            }

            claim.UserID = userId;

            // Require PDF upload
            if (pdfFile == null || pdfFile.Length == 0)
            {
                ModelState.AddModelError("RatePDF", "PDF is required.");
                ViewData["UserID"] = userId;
                return View(claim);
            }

            // File validation
            if (pdfFile != null && pdfFile.Length > 0)
            {
                if (!Path.GetExtension(pdfFile.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("RatePDF", "Only PDF files are allowed.");
                    ViewData["UserID"] = userId;
                    return View(claim);
                }

                if (!string.Equals(pdfFile.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("RatePDF", "Invalid file type. Must be a PDF.");
                    ViewData["UserID"] = userId;
                    return View(claim);
                }

                const long maxBytes = 10 * 1024 * 1024; // 10 MB
                if (pdfFile.Length > maxBytes)
                {
                    ModelState.AddModelError("RatePDF", "PDF is too large (max 10MB).");
                    ViewData["UserID"] = userId;
                    return View(claim);
                }

                using (var memoryStream = new MemoryStream())
                {
                    await pdfFile.CopyToAsync(memoryStream);
                    claim.RatePDF = memoryStream.ToArray();
                }
            }

            claim.Status = "Pending";
            claim.DateCreated = DateTime.Now;

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            return RedirectToAction("ClaimSuccess", "Home");
        }

        // GET: Claims/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                return NotFound();
            }
            ViewData["UserID"] = new SelectList(_context.Users, "UserID", "UserName", claim.UserID);
            return View(claim);
        }

        // POST: Claims/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Hours,Rate,UserID,Status")] Claim claim, IFormFile? RatePDFFile)
        {
            if (id != claim.ID)
            {
                return NotFound();
            }

            var existingClaim = await _context.Claims.AsNoTracking().FirstOrDefaultAsync(c => c.ID == id);
            if (existingClaim == null)
            {
                return NotFound();
            }

            // Preserve DateCreated from existing claim
            claim.DateCreated = existingClaim.DateCreated;

            if (RatePDFFile != null && RatePDFFile.Length > 0)
            {
                if (!Path.GetExtension(RatePDFFile.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("RatePDF", "Only PDF files are allowed.");
                    ViewData["UserID"] = new SelectList(_context.Users, "UserID", "UserName", claim.UserID);
                    return View(claim);
                }
                if (!string.Equals(RatePDFFile.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("RatePDF", "Invalid file type. Must be a PDF.");
                    ViewData["UserID"] = new SelectList(_context.Users, "UserID", "UserName", claim.UserID);
                    return View(claim);
                }
                const long maxBytesEdit = 10 * 1024 * 1024; // 10 MB
                if (RatePDFFile.Length > maxBytesEdit)
                {
                    ModelState.AddModelError("RatePDF", "PDF is too large (max 10MB).");
                    ViewData["UserID"] = new SelectList(_context.Users, "UserID", "UserName", claim.UserID);
                    return View(claim);
                }

                using (var memoryStream = new MemoryStream())
                {
                    await RatePDFFile.CopyToAsync(memoryStream);
                    claim.RatePDF = memoryStream.ToArray();
                }
            }
            else
            {
                // Preserve existing PDF when none uploaded
                claim.RatePDF = existingClaim.RatePDF;
            }

            try
            {
                _context.Update(claim);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClaimExists(claim.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("Index", "Claims");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (!string.Equals(role, "Coordinator", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Claims");
            }

            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                return NotFound();
            }

            if (!string.Equals(claim.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Details", "Claims", new { id = claim.ID });
            }

            claim.Status = "Verified";
            _context.Update(claim);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Claims", new { id = claim.ID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (!string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Claims");
            }

            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
                return NotFound();

            // Only approve if Verified; otherwise redirect back to details
            if (!string.Equals(claim.Status, "Verified", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Details", "Claims", new { id = claim.ID });
            }

            claim.Status = "Approved";

            _context.Update(claim);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Claims", new { id = claim.ID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decline(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
                return NotFound();

            claim.Status = claim.Status == "Declined" ? "Pending" : "Declined";
            TempData["Message"] = claim.Status == "Declined"
                ? "Claim declined."
                : "Claim reverted to Pending.";

            _context.Update(claim);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Claims", new { id = claim.ID });
        }

        // GET: Claims/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claim = await _context.Claims
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (claim == null)
            {
                return NotFound();
            }

            return View(claim);
        }

        // POST: Claims/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim != null)
            {
                _context.Claims.Remove(claim);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Claims/DownloadPdf/5
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var claim = await _context.Claims.FirstOrDefaultAsync(c => c.ID == id);
            if (claim == null || claim.RatePDF == null || claim.RatePDF.Length == 0)
            {
                return NotFound();
            }
            return File(claim.RatePDF, "application/pdf", $"claim_{id}.pdf");
        }

        private bool ClaimExists(int id)
        {
            return _context.Claims.Any(e => e.ID == id);
        }

        // GET: Claims/Track - auto-load for logged-in user
        public async Task<IActionResult> Track()
        {
            var userIdString = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                TempData["Error"] = "You must be logged in to track your claims.";
                return RedirectToAction("Login", "Home");
            }

            var userClaims = await _context.Claims
                .Where(c => c.UserID == userId)
                .Include(c => c.User)
                .ToListAsync();

            if (userClaims == null || !userClaims.Any())
            {
                ViewBag.Message = "No claims found for your account.";
                return View(new List<Claim>());
            }

            return View(userClaims);
        }

        // POST: Claims/Track (kept for backward compatibility, but not required)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Track(int userId)
        {
            var userClaims = await _context.Claims
                .Where(c => c.UserID == userId)
                .Include(c => c.User)
                .ToListAsync();

            if (userClaims == null || !userClaims.Any())
            {
                ViewBag.Message = "No claims found for the given User ID.";
                return View(new List<Claim>());
            }

            return View(userClaims);
        }
    }
}
