// Controllers/TripsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList
using Microsoft.EntityFrameworkCore; // For DbUpdateConcurrencyException
using TFMS.Data;
using TFMS.Models;
using TFMS.Services;

namespace TFMS.Controllers
{
    [Authorize] // All actions require authentication
    public class TripsController : Controller
    {
        private readonly ITripService _tripService;
        private readonly IVehicleService _vehicleService; // To get list of vehicles for dropdowns
        private readonly ApplicationDbContext _context; // To get list of drivers for dropdowns (UserManager would be another option)

        // Constructor injection
        public TripsController(ITripService tripService, IVehicleService vehicleService, ApplicationDbContext context)
        {
            _tripService = tripService;
            _vehicleService = vehicleService;
            _context = context;
        }

        // Helper method to populate Vehicle and Driver dropdowns
       
        private async Task PopulateDropdowns(int? selectedVehicle = null, string? selectedDriver = null)
        {
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber", selectedVehicle);

            // Get only active drivers for the dropdown
            var drivers = await _context.Users
                                        .Where(u => u.IsActiveDriver) // Assuming IsActiveDriver property in ApplicationUser
                                        .ToListAsync();
            ViewBag.DriverId = new SelectList(drivers, "Id", "Email", selectedDriver); // Using Id for value, Email for display
        }

        // GET: Trips
        // Fleet Administrator and Fleet Operator can view all trips
        [Authorize(Roles = "Fleet Administrator,Fleet Operator")]
        public async Task<IActionResult> Index()
        {
            var trips = await _tripService.GetAllTripsAsync();
            return View(trips);
        }

        // GET: Trips/Details/5
        // Fleet Administrator and Fleet Operator can view trip details
        [Authorize(Roles = "Fleet Administrator,Fleet Operator")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _tripService.GetTripByIdAsync(id.Value);
            if (trip == null)
            {
                return NotFound();
            }

            return View(trip);
        }

        // GET: Trips/Create
        // Only Fleet Administrator can create new trips
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        // POST: Trips/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Create([Bind("VehicleId,DriverId,StartLocation,EndLocation,ScheduledStartTime,ScheduledEndTime,Status,EstimatedDistanceKm,RouteDetails")] Trip trip)
        {
            // Remove navigation properties from ModelState to prevent validation errors on FKs
            // The bound properties (VehicleId, DriverId) are sufficient
            ModelState.Remove("Vehicle");
            ModelState.Remove("Driver");

            if (ModelState.IsValid)
            {
                await _tripService.AddTripAsync(trip);
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdowns(trip.VehicleId, trip.DriverId); // Repopulate dropdowns on error
            return View(trip);
        }

        // GET: Trips/Edit/5
        // Only Fleet Administrator can edit trips
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _tripService.GetTripByIdAsync(id.Value); // Use service to get by ID, which includes Vehicle/Driver
            if (trip == null)
            {
                return NotFound();
            }

            await PopulateDropdowns(trip.VehicleId, trip.DriverId);
            return View(trip);
        }

        // POST: Trips/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("TripId,VehicleId,DriverId,StartLocation,EndLocation,ScheduledStartTime,ScheduledEndTime,Status,ActualStartTime,ActualEndTime,EstimatedDistanceKm,ActualDistanceKm,RouteDetails")] Trip trip)
        {
            if (id != trip.TripId)
            {
                return NotFound();
            }

            // Remove navigation properties from ModelState
            ModelState.Remove("Vehicle");
            ModelState.Remove("Driver");

            if (ModelState.IsValid)
            {
                try
                {
                    await _tripService.UpdateTripAsync(trip);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _tripService.TripExistsAsync(trip.TripId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdowns(trip.VehicleId, trip.DriverId); // Repopulate dropdowns on error
            return View(trip);
        }

        // GET: Trips/Delete/5
        // Only Fleet Administrator can delete trips
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _tripService.GetTripByIdAsync(id.Value);
            if (trip == null)
            {
                return NotFound();
            }

            return View(trip);
        }

        // POST: Trips/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _tripService.DeleteTripAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Action for Driver to update trip status (More restricted than Edit)
        // Can be accessed by Fleet Administrator, Fleet Operator, and Driver
        [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")]
        public async Task<IActionResult> UpdateStatus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _tripService.GetTripByIdAsync(id.Value);
            if (trip == null)
            {
                return NotFound();
            }

            // Ensure only the assigned driver or admin/operator can update status
            if (User.IsInRole("Driver") && trip.DriverId != User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value)
            {
                return Forbid(); // Driver trying to update another driver's trip
            }

            return View(trip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")]
        public async Task<IActionResult> UpdateStatus(int id, [Bind("TripId,Status,ActualStartTime,ActualEndTime,ActualDistanceKm")] Trip tripUpdate)
        {
            if (id != tripUpdate.TripId)
            {
                return NotFound();
            }

            var existingTrip = await _tripService.GetTripByIdAsync(id);
            if (existingTrip == null)
            {
                return NotFound();
            }

            // Only allow the assigned driver or admin/operator to update status
            if (User.IsInRole("Driver") && existingTrip.DriverId != User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value)
            {
                return Forbid(); // Driver trying to update another driver's trip
            }

            // Update only allowed fields for status update
            existingTrip.Status = tripUpdate.Status;
            existingTrip.ActualStartTime = tripUpdate.ActualStartTime ?? existingTrip.ActualStartTime; // Update only if provided
            existingTrip.ActualEndTime = tripUpdate.ActualEndTime ?? existingTrip.ActualEndTime;
            existingTrip.ActualDistanceKm = tripUpdate.ActualDistanceKm ?? existingTrip.ActualDistanceKm;


            // If status is "In Progress" and ActualStartTime is null, set it
            if (existingTrip.Status == "In Progress" && !existingTrip.ActualStartTime.HasValue)
            {
                existingTrip.ActualStartTime = DateTime.Now;
            }
            // If status is "Completed" and ActualEndTime is null, set it
            else if (existingTrip.Status == "Completed" && !existingTrip.ActualEndTime.HasValue)
            {
                existingTrip.ActualEndTime = DateTime.Now;
            }


            // Remove navigation properties from ModelState
            ModelState.Remove("Vehicle");
            ModelState.Remove("Driver");


            if (ModelState.IsValid)
            {
                try
                {
                    await _tripService.UpdateTripAsync(existingTrip);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _tripService.TripExistsAsync(existingTrip.TripId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = existingTrip.TripId }); // Redirect to details after update
            }

            // If ModelState is invalid, re-show the form
            return View(tripUpdate);
        }
    }
}