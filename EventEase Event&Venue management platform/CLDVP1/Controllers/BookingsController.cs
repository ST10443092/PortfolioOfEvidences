using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CLDV1.Models;
using CLDVP1.Data;
using Microsoft.Data.SqlClient;
using Azure.Storage.Blobs;

namespace CLDVP1.Controllers
{
    public class BookingsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        // Inject IConfiguration via constructor
        public BookingsController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .ToListAsync();
            return View(bookings);
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get booking with related data in single query
            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .Include(b => b.EventType)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Hybrid image handling
            if (booking.Venue != null)
            {
                // Case 1: Image URL already in database
                if (!string.IsNullOrEmpty(booking.Venue.ImageUrl))
                {
                    // Verify the stored URL is still valid
                    if (!await BlobExists(booking.Venue.ImageUrl))
                    {
                        booking.Venue.ImageUrl = await GetVenueImageUrl(booking.Venue.VenueId);
                        _context.Update(booking.Venue);
                        await _context.SaveChangesAsync();
                    }
                }
                // Case 2: Fetch from blob storage if no URL in DB
                else
                {
                    var imageUrl = await GetVenueImageUrl(booking.Venue.VenueId);
                    if (imageUrl != null)
                    {
                        booking.Venue.ImageUrl = imageUrl;
                        _context.Update(booking.Venue);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return View(booking);
        }

        // Helper method to check blob existence
        private async Task<bool> BlobExists(string imageUrl)
        {
            try
            {
                var blobClient = new BlobClient(new Uri(imageUrl));
                return await blobClient.ExistsAsync();
            }
            catch
            {
                return false;
            }
        }

        // Helper method to get image URL from blob storage
        private async Task<string?> GetVenueImageUrl(int venueId)
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(
                    _configuration.GetConnectionString("AzureBlobStorage"));
                var containerClient = blobServiceClient.GetBlobContainerClient("venue-images7");
                var blobClient = containerClient.GetBlobClient(venueId.ToString());

                return await blobClient.ExistsAsync() ? blobClient.Uri.ToString() : null;
            }
            catch
            {
                return null;
            }
        }

        // GET: Bookings/Create
        public async Task<IActionResult> Create()
        {
            //ViewBag.EventTypes = new SelectList(_context.EventTypes.OrderBy(et => et.TypeName),
            //                         "EventTypeId",
            //                         "TypeName");
            await PopulateDropdownsAsync();
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,BookingDate,EventId,VenueId,EventTypeId")] Booking booking)
        {
            //if (ModelState.IsValid)
            //{
            //    try
            //    {
                    // Verify the referenced entities exist
                    var eventExists = await _context.Event.AnyAsync(e => e.EventId == booking.EventId);
                    var venueExists = await _context.Venue.AnyAsync(v => v.VenueId == booking.VenueId);
                    var EventTypeExists = await _context.EventTypes.AnyAsync(v => v.EventTypeId == booking.EventTypeId);

            //if (!eventExists || !venueExists)
            //{
            //    ModelState.AddModelError("", "Selected event or venue does not exist");
            //    await PopulateDropdownsAsync();
            //    return View(booking);
            //}
            // Check for existing booking at same venue and date
            bool isAlreadyBooked = await _context.Booking
         .AnyAsync(b => b.VenueId == booking.VenueId &&
                       b.BookingDate.Date == booking.BookingDate.Date);

            if (isAlreadyBooked)
            {
                ModelState.AddModelError("BookingDate", "This venue is already booked for the selected date");
                return View(booking);
            }
            else
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            //    }
            //    catch (DbUpdateException ex) when (IsForeignKeyViolation(ex))
            //    {
            //        ModelState.AddModelError("", "Invalid event or venue selected. Please verify your choices.");
            //    }
            //    catch (Exception)
            //    {
            //        ModelState.AddModelError("", "An error occurred while saving. Please try again.");
            //    }
            //}

            //await PopulateDropdownsAsync();
            //return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var booking = await _context.Booking.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            await PopulateDropdownsAsync();
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,BookingDate,EventId,VenueId,EventTypeId")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

              
                    // Verify the referenced entities exist
                    var eventExists = await _context.Event.AnyAsync(e => e.EventId == booking.EventId);
                    var venueExists = await _context.Venue.AnyAsync(v => v.VenueId == booking.VenueId);

                   

                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
             
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            return booking == null ? NotFound() : View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking != null)
            {
                _context.Booking.Remove(booking);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.BookingId == id);
        }

        private async Task PopulateDropdownsAsync()
        {
            ViewData["EventId"] = new SelectList(await _context.Event.ToListAsync(), "EventId", "EventName");
            ViewData["VenueId"] = new SelectList(await _context.Venue.ToListAsync(), "VenueId", "Name");
            ViewBag.EventTypes = new SelectList(_context.EventTypes.OrderBy(et => et.TypeName),
                                     "EventTypeId",
                                     "TypeName");
        }
        //public async Task<IActionResult> EnhancedView(string searchTerm)
        //{
        //    var query = _context.Booking.AsQueryable();

        //    if (!string.IsNullOrEmpty(searchTerm))
        //    {
        //        query = query.Where(b =>
        //            b.BookingId.ToString().Contains(searchTerm) ||
        //            b.EventName.Contains(searchTerm));
        //    }

        //    var model = await query
        //        .OrderBy(b => b.BookingDate)
        //        .ToListAsync();

        //    return View(model);
        //}
        // In your BookingsController.cs
        public async Task<IActionResult> Search(
     string searchTerm,
     int? eventTypeId,
     DateTime? startDate,
     DateTime? endDate,
     bool? onlyAvailable)
        {
            // Populate EventTypes dropdown
            ViewBag.EventTypes = new SelectList(
                await _context.EventTypes.OrderBy(et => et.TypeName).ToListAsync(),
                "EventTypeId",
                "TypeName",
                eventTypeId); // Preserves selected value during search

            var query = _context.Booking
                .Include(b => b.Event)
                    .ThenInclude(e => e.EventType)
                .Include(b => b.Venue)
                .AsQueryable();

            // Keyword search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b =>
                    b.BookingId.ToString().Contains(searchTerm) ||
                    (b.Event != null && b.Event.EventName.Contains(searchTerm)));
            }

            // Filter by EventType
            if (eventTypeId.HasValue)
            {
                query = query.Where(b => b.Event.EventTypeId == eventTypeId);
            }

            // Filter by Event date range
            if (startDate.HasValue)
            {
                query = query.Where(b => b.Event.EventDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(b => b.Event.EventDate <= endDate.Value);
            }

            // Filter by venue availability
            if (onlyAvailable.HasValue && onlyAvailable.Value)
            {
                query = query.Where(b => b.Venue.IsAvailable);
            }

            var bookings = await query
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }

        private bool IsForeignKeyViolation(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx && sqlEx.Number == 547;
        }
    }
}