using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CLDV1.Models;
using CLDVP1.Data;

namespace CLDVP1.Controllers
{
    public class EventsController : Controller
    {
        private readonly AppDbContext _context;

        public EventsController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("Any")]
        public IActionResult AnyEvent()
        {
            return Json(new { exists = _context.Event.Any() });
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Event.Include(b => b.Venue);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Event
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "Name"); // <- use Name, not ImageUrl
            ViewBag.EventTypes = new SelectList(_context.EventTypes.OrderBy(et => et.TypeName),
                                      "EventTypeId",
                                      "TypeName"); // <- use TypeName for EventType
            return View();
        }

        // POST: Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,EventName,EventDate,Description,VenueId,EventName,EventTypeId,")] Event @event)
        {

           // ViewBag.EventTypes = new SelectList(_context.EventTypes, "EventTypeId", "TypeName");
            _context.Add(@event);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");


        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event.FindAsync(id);
            if (@event == null) return NotFound();

            ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "Name", @event.VenueId); // pre-select
            ViewBag.EventTypes = new SelectList(_context.EventTypes.OrderBy(et => et.TypeName),
                          "EventTypeId",
                          "TypeName"); // <- use TypeName for EventType
            return View(@event);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,EventDate,Description,VenueId,EventTypeId")] Event @event)
        {


            ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "Name", @event.VenueId);
            ViewBag.EventTypes = new SelectList(_context.EventTypes.OrderBy(et => et.TypeName),
                           "EventTypeId",
                           "TypeName"); // <- use TypeName for EventType
            _context.Update(@event);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");




           

        }

        // GET: Events/Delete/5
        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Event
                .Include(e => e.Venue)
                .Include(e => e.Booking) // Include bookings to check for any association
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null)
            {
                return NotFound();
            }

            // Check if event has any bookings
            if (@event.Booking?.Count > 0)
            {
                ViewBag.DeleteWarning = "This event cannot be deleted because it has associated bookings.";
                ViewBag.CanDelete = false;
            }
            else
            {
                ViewBag.CanDelete = true;
            }

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Event
                .Include(e => e.Booking) // Include bookings for validation
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (@event == null)
            {
                return NotFound();
            }

            // Check if event has any bookings
            if (@event.Booking?.Count > 0)
            {
                // Return to delete view with error
                @event = await _context.Event
                    .Include(e => e.Venue)
                    .Include(e => e.Booking)
                    .FirstOrDefaultAsync(e => e.EventId == id);

                ModelState.AddModelError(string.Empty,
                    "Deletion failed. This event has associated bookings and cannot be deleted.");
                ViewBag.CanDelete = false;
                return View("Delete", @event);
            }

            // Proceed with deletion if no bookings exist
            _context.Event.Remove(@event);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Events/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var @event = await _context.Event.FindAsync(id);
        //    if (@event != null)
        //    {
        //        _context.Event.Remove(@event);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}

        private bool EventExists(int id)
        {
            return _context.Event.Any(e => e.EventId == id);
        }
    }
}
