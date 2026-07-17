using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CLDV1.Models;
using CLDVP1.Data;
using Azure.Storage.Blobs;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using CLDVP1.Interfaces;

namespace CLDVP1.Controllers
{
    public class VenuesController : Controller
    {
        private readonly AppDbContext _context;

       
        private readonly IConfiguration _configuration;

        public VenuesController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Venues
        public async Task<IActionResult> Index()
        {
            return View(await _context.Venue.ToListAsync());
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }
        [HttpGet("Any")]
        public IActionResult AnyVenue()
        {
            return Json(new { exists = _context.Venue.Any() });
        }

        // GET: Venues/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venues/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Location,Capacity,ImageFile")] Venue venue)
        {
            if (venue.ImageFile != null && venue.ImageFile.Length > 0)
            {
                // Step 1: Upload to Azure Blob Storage
                var blobServiceClient = new BlobServiceClient(
                    _configuration.GetConnectionString("AzureBlobStorage"));
                var containerClient = blobServiceClient.GetBlobContainerClient("images7");

                // Generate unique blob name (using VenueId + extension)
                string blobName = $"{Guid.NewGuid()}{Path.GetExtension(venue.ImageFile.FileName)}";
                var blobClient = containerClient.GetBlobClient(blobName);

                // Upload the file
                using (var stream = venue.ImageFile.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream);
                }

                // Step 2: Store the URL in database
                venue.ImageUrl = blobClient.Uri.ToString();
            }

            _context.Add(venue);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return View(venue);
        }

        // POST: Venues/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,Location,Capacity,Name,ImageUrl")] Venue venue, IFormFile imageFile)
        {
            if (id != venue.VenueId)
            {
                return NotFound();
            }

            var existingVenue = await _context.Venue.FindAsync(id);
            if (existingVenue == null)
            {
                return NotFound();
            }

            // Update properties
            //existingVenue.Location = venue.Location;
            //existingVenue.Capacity = venue.Capacity;
            //existingVenue.Name = venue.Name;
            //existingVenue.ImageUrl = venue.ImageUrl;
            if (venue.ImageFile != null && venue.ImageFile.Length > 0)
            {
                // Step 1: Upload to Azure Blob Storage
                var blobServiceClient = new BlobServiceClient(
                    _configuration.GetConnectionString("AzureBlobStorage"));
                var containerClient = blobServiceClient.GetBlobContainerClient("images7");

                // Generate unique blob name (using VenueId + extension)
                string blobName = $"{Guid.NewGuid()}{Path.GetExtension(venue.ImageFile.FileName)}";
                var blobClient = containerClient.GetBlobClient(blobName);

                // Upload the file
                using (var stream = venue.ImageFile.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream);
                }

                // Step 2: Store the URL in database
                venue.ImageUrl = blobClient.Uri.ToString();
            }
            existingVenue.Location = venue.Location;
            existingVenue.Capacity = venue.Capacity;
            existingVenue.Name = venue.Name;
          

            _context.Update(existingVenue);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        


       
            
          
        }

        // GET: Venues/Delete/5
        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue
                .Include(v => v.Events) // Include related events
                .ThenInclude(e => e.Booking) // Include bookings for those events
                .FirstOrDefaultAsync(m => m.VenueId == id);

            if (venue == null)
            {
                return NotFound();
            }

            // Check if venue has any associated events
            bool hasEvents = venue.Events?.Count > 0;
            bool hasBookings = venue.Events?.Any(e => e.Booking?.Count > 0) ?? false;

            if (hasEvents || hasBookings)
            {
                string message = hasBookings ?
                    "This venue cannot be deleted because it has events with associated bookings." :
                    "This venue cannot be deleted because it has associated events.";

                ViewBag.DeleteWarning = message;
                ViewBag.CanDelete = false;
            }
            else
            {
                ViewBag.CanDelete = true;
            }

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue
                .Include(v => v.Events)
                .ThenInclude(e => e.Booking)
                .FirstOrDefaultAsync(v => v.VenueId == id);

            if (venue == null)
            {
                return NotFound();
            }

            // Check for associated events or bookings
            bool hasEvents = venue.Events?.Count > 0;
            bool hasBookings = venue.Events?.Any(e => e.Booking?.Count > 0) ?? false;

            if (hasEvents || hasBookings)
            {
                string message = hasBookings ?
                    "Deletion failed. Venue has events with associated bookings." :
                    "Deletion failed. Venue has associated events.";

                ModelState.AddModelError(string.Empty, message);
                ViewBag.CanDelete = false;
                return View("Delete", venue);
            }

            // Proceed with deletion if no dependencies exist
            _context.Venue.Remove(venue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //// POST: Venues/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var venue = await _context.Venue.FindAsync(id);
        //    if (venue != null)
        //    {
        //        _context.Venue.Remove(venue);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}

        private bool VenueExists(int id)
        {
            return _context.Venue.Any(e => e.VenueId == id);
        }
    }
}
