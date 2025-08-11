using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SalonManager.Models;

namespace SalonManager.Controllers
{
    public class BookingsController : Controller
    {
        private readonly AppDbContext _context;

        public BookingsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            var bookings = _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Service)
                .Include(b => b.Staff);
            return View(await bookings.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Service)
                .Include(b => b.Staff)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            PopulateStatusDropdown();
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CustomerId,ServiceId,StaffId,StartAt,EndAt,Status,Notes")] Booking booking)
        {
            if (!ModelState.IsValid || !IsBookingTimeAvailable(booking))
            {
                if (!IsBookingTimeAvailable(booking))
                {
                    ModelState.AddModelError("", "Selected staff is not available for the chosen time slot.");
                }
                PopulateDropdowns(booking);
                PopulateStatusDropdown(booking.Status);
                return View(booking);
            }

            _context.Add(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();

            PopulateDropdowns(booking);
            PopulateStatusDropdown(booking.Status);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerId,ServiceId,StaffId,StartAt,EndAt,Status,Notes")] Booking booking)
        {
            if (id != booking.Id)
                return NotFound();

            if (!ModelState.IsValid || !IsBookingTimeAvailable(booking))
            {
                if (!IsBookingTimeAvailable(booking))
                {
                    ModelState.AddModelError("", "Selected staff is not available for the chosen time slot.");
                }
                PopulateDropdowns(booking);
                PopulateStatusDropdown(booking.Status);
                return View(booking);
            }

            try
            {
                _context.Update(booking);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(booking.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Service)
                .Include(b => b.Staff)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }

        // Helper to populate dropdown lists for Customer, Service, Staff
        private void PopulateDropdowns(Booking? booking = null)
        {
            ViewBag.Customers = new SelectList(_context.Users, "Id", "UserName", booking?.CustomerId);
            ViewBag.Services = new SelectList(_context.Services, "Id", "Name", booking?.ServiceId);
            ViewBag.Staffs = new SelectList(_context.Staffs.Where(s => s.IsActive), "Id", "Name", booking?.StaffId);
        }

        // Helper to populate dropdown for BookingStatus enum
        private void PopulateStatusDropdown(BookingStatus? selectedStatus = null)
        {
            ViewBag.Statuses = new SelectList(Enum.GetValues(typeof(BookingStatus)), selectedStatus);
        }

        // Simple booking time conflict checker (only checks staff availability)
        private bool IsBookingTimeAvailable(Booking booking)
        {
            if (!booking.StaffId.HasValue)
                return true; // No staff assigned, assume available

            var conflictingBooking = _context.Bookings
                .Where(b => b.Id != booking.Id && b.StaffId == booking.StaffId && b.Status != BookingStatus.Cancelled)
                .FirstOrDefault(b =>
                    (booking.StartAt < b.EndAt) && (booking.EndAt > b.StartAt)
                );

            return conflictingBooking == null;
        }
    }
}
