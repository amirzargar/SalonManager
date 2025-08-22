using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SalonManager.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SalonManager.Controllers
{
    [Authorize(Roles = "Admin,Staff,Customer")]
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
            if (User.IsInRole("Admin"))
            {
                return View(await _context.Bookings
                    .Include(x => x.Service)
                    .Include(x => x.Staff)
                    .Include(x => x.Customer)
                    .ToListAsync());
            }
            else if (User.IsInRole("Staff"))
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
                var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
                var staffId = staff?.Id ?? 0;
                return View(await _context.Bookings
                    .Where(b => b.StaffId == staffId)
                    .Include(x => x.Service)
                    .Include(x => x.Customer)
                    .ToListAsync());
            }
            else // Customer
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
                return View(await _context.Bookings
                    .Where(b => b.CustomerId == userId)
                    .Include(x => x.Service)
                    .Include(x => x.Staff)
                    .ToListAsync());
            }
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

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            if (User.IsInRole("Admin")) { }
            else if (User.IsInRole("Staff"))
            {
                var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
                if (booking.StaffId != staff?.Id)
                    return Forbid();
            }
            else if (User.IsInRole("Customer"))
            {
                if (booking.CustomerId != userId)
                    return Forbid();
            }
            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            PopulateDropdowns();

            if (User.IsInRole("Admin"))
            {
                ViewBag.Customers = new SelectList(_context.Users, "Id", "Email");
            }

            PopulateStatusDropdown();
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ServiceId,StaffId,StartAt,EndAt,Status,Notes,CustomerId")] Booking booking)
        {
            if (!User.IsInRole("Admin"))
            {
                booking.CustomerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
                // Set default status to Pending for customers creating bookings
                booking.Status = BookingStatus.Pending;

                ModelState.Remove(nameof(booking.CustomerId));
                ModelState.Remove(nameof(booking.Status));
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = "Model validation failed: " + errors;

                PopulateDropdowns(booking);
                if (User.IsInRole("Admin"))
                    ViewBag.Customers = new SelectList(_context.Users, "Id", "Email", booking.CustomerId);
                PopulateStatusDropdown(booking.Status);
                return View(booking);
            }

            if (!IsBookingTimeAvailable(booking))
            {
                TempData["ErrorMessage"] = "Selected staff is not available for the chosen time slot.";
                PopulateDropdowns(booking);
                if (User.IsInRole("Admin"))
                    ViewBag.Customers = new SelectList(_context.Users, "Id", "Email", booking.CustomerId);
                PopulateStatusDropdown(booking.Status);
                return View(booking);
            }

            _context.Add(booking);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Booking created successfully.";
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

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            if (User.IsInRole("Admin")) { }
            else if (User.IsInRole("Staff"))
            {
                var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
                if (booking.StaffId != staff?.Id)
                    return Forbid();
            }
            else if (User.IsInRole("Customer"))
            {
                if (booking.CustomerId != userId)
                    return Forbid();
            }
            PopulateDropdowns(booking);
            if (User.IsInRole("Admin"))
            {
                ViewBag.Customers = new SelectList(_context.Users, "Id", "Email", booking.CustomerId);
            }
            PopulateStatusDropdown(booking.Status);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ServiceId,StaffId,StartAt,EndAt,Status,Notes,CustomerId")] Booking booking)
        {
            if (id != booking.Id)
                return NotFound();

            var originalBooking = await _context.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
            if (originalBooking == null)
                return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;

            if (!User.IsInRole("Admin"))
            {
                if (User.IsInRole("Staff"))
                {
                    var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
                    if (originalBooking.StaffId != staff?.Id)
                        return Forbid();
                }
                else if (User.IsInRole("Customer"))
                {
                    if (originalBooking.CustomerId != userId)
                        return Forbid();

                    booking.CustomerId = originalBooking.CustomerId;
                    booking.Status = originalBooking.Status; // Prevent status change by customer
                    
                    ModelState.Remove(nameof(booking.CustomerId));
                    ModelState.Remove(nameof(booking.Status));
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(booking.CustomerId))
                {
                    ModelState.AddModelError("CustomerId", "The Customer field is required.");
                }
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = "Edit validation failed: " + errors;

                PopulateDropdowns(booking);
                if (User.IsInRole("Admin"))
                {
                    ViewBag.Customers = new SelectList(_context.Users, "Id", "Email", booking.CustomerId);
                }
                PopulateStatusDropdown(booking.Status);
                return View(booking);
            }

            if (!IsBookingTimeAvailable(booking))
            {
                ModelState.AddModelError("", "Selected staff is not available for the chosen time slot.");
                PopulateDropdowns(booking);
                if (User.IsInRole("Admin"))
                {
                    ViewBag.Customers = new SelectList(_context.Users, "Id", "Email", booking.CustomerId);
                }
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

            TempData["SuccessMessage"] = "Booking updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Bookings/EditStatus/5
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> EditStatus(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Service)
                .Include(b => b.Staff)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
            if (booking.StaffId != staff?.Id)
                return Forbid();

            // Only allow status and notes editing in view
            ViewBag.Statuses = new SelectList(Enum.GetValues(typeof(BookingStatus)), booking.Status);
            return View(booking);
        }

        // POST: Bookings/EditStatus/5
        [HttpPost]
        [Authorize(Roles = "Staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(int id, [Bind("Id,Status,Notes")] Booking updateBooking)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null)
                return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
            if (booking.StaffId != staff?.Id)
                return Forbid();

            // Only update allowed fields
            booking.Status = updateBooking.Status;
            booking.Notes = updateBooking.Notes;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Booking status updated.";
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

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            if (User.IsInRole("Admin")) { }
            else if (User.IsInRole("Staff"))
            {
                var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
                if (booking.StaffId != staff?.Id)
                    return Forbid();
            }
            else if (User.IsInRole("Customer"))
            {
                if (booking.CustomerId != userId)
                    return Forbid();
            }
            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;

            if (User.IsInRole("Staff"))
            {
                return Forbid();
            }

            if (User.IsInRole("Customer") && booking.CustomerId != userId)
            {
                return Forbid();
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Booking deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }

        private void PopulateDropdowns(Booking? booking = null)
        {
            ViewBag.Services = new SelectList(_context.Services, "Id", "Name", booking?.ServiceId);
            ViewBag.Staffs = new SelectList(_context.Staffs.Where(s => s.IsActive), "Id", "Name", booking?.StaffId);
        }

        private void PopulateStatusDropdown(BookingStatus? selectedStatus = null)
        {
            ViewBag.Statuses = new SelectList(Enum.GetValues(typeof(BookingStatus)), selectedStatus);
        }

        private bool IsBookingTimeAvailable(Booking booking)
        {
            if (!booking.StaffId.HasValue)
                return true;
            var conflictingBooking = _context.Bookings
                .Where(b => b.Id != booking.Id && b.StaffId == booking.StaffId && b.Status != BookingStatus.Cancelled)
                .FirstOrDefault(b =>
                    booking.StartAt < b.EndAt && booking.EndAt > b.StartAt);
            return conflictingBooking == null;
        }
    }
}
