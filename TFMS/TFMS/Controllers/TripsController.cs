// Controllers/TripsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TFMS.Data;
using TFMS.Models;
using TFMS.Services;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Security.Claims;

namespace TFMS.Controllers
{
    [Authorize]
    public class TripsController : Controller
    {
        private readonly ILogger<TripsController> _logger;
        private readonly ITripService _tripService;
        private readonly IVehicleService _vehicleService;
        private readonly ApplicationDbContext _context;

        public TripsController(
            ILogger<TripsController> logger,
            ITripService tripService,
            IVehicleService vehicleService,
            ApplicationDbContext context)
        {
            _logger = logger;
            _tripService = tripService;
            _vehicleService = vehicleService;
            _context = context;
        }

        // Helper method to populate Vehicle and Driver dropdowns
        private async Task PopulateDropdowns(int? selectedVehicle = null, string? selectedDriver = null)
        {
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber", selectedVehicle);
            var drivers = await _context.Users.Where(u => u.IsActiveDriver).ToListAsync();
            ViewBag.DriverId = new SelectList(drivers, "Id", "Email", selectedDriver);
        }

        // GET: Trips
        [Authorize(Roles = "Fleet Administrator,Fleet Operator")]
        public async Task<IActionResult> Index(string? searchString, string? statusFilter, int? vehicleIdFilter, string? driverIdFilter)
        {
            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentStatusFilter = statusFilter;
            ViewBag.CurrentVehicleFilter = vehicleIdFilter;
            ViewBag.CurrentDriverFilter = driverIdFilter;

            var statusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "All", Text = "All" }
            };
            foreach (TripStatus statusEnum in Enum.GetValues(typeof(TripStatus)))
            {
                statusOptions.Add(new SelectListItem { Value = statusEnum.ToString(), Text = statusEnum.GetDescription() });
            }
            ViewBag.StatusFilter = new SelectList(statusOptions, "Value", "Text", statusFilter);

            var allVehicles = await _vehicleService.GetAllVehiclesAsync();
            var vehicleListItems = allVehicles.Select(v => new SelectListItem { Value = v.VehicleId.ToString(), Text = v.RegistrationNumber }).ToList();
            vehicleListItems.Insert(0, new SelectListItem { Value = "0", Text = "All Vehicles" });
            ViewBag.VehicleFilter = new SelectList(vehicleListItems, "Value", "Text", vehicleIdFilter?.ToString());

            var allDrivers = await _context.Users.Where(u => u.IsActiveDriver).ToListAsync();
            var driverListItems = allDrivers.Select(d => new SelectListItem { Value = d.Id, Text = d.Email }).ToList();
            driverListItems.Insert(0, new SelectListItem { Value = "0", Text = "All Drivers" });
            ViewBag.DriverFilter = new SelectList(driverListItems, "Value", "Text", driverIdFilter);

            var trips = await _tripService.GetAllTripsAsync(searchString, statusFilter, vehicleIdFilter, driverIdFilter);
            return View(trips);
        }

        // GET: Trips/Details/5
        [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var trip = await _tripService.GetTripByIdAsync(id.Value);
            if (trip == null) return NotFound();

            if (User.IsInRole("Driver") && trip.DriverId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            {
                return Forbid();
            }

            return View(trip);
        }

        // GET: Trips/Create
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
            ModelState.Remove("Vehicle");
            ModelState.Remove("Driver");

            if (ModelState.IsValid)
            {
                await _tripService.AddTripAsync(trip);
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdowns(trip.VehicleId, trip.DriverId);
            return View(trip);
        }

        // GET: Trips/Edit/5
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var trip = await _tripService.GetTripByIdAsync(id.Value);
            if (trip == null) return NotFound();

            await PopulateDropdowns(trip.VehicleId, trip.DriverId);
            return View(trip);
        }

        // POST: Trips/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("TripId,VehicleId,DriverId,StartLocation,EndLocation,ScheduledStartTime,ScheduledEndTime,Status,ActualStartTime,ActualEndTime,EstimatedDistanceKm,ActualDistanceKm,RouteDetails")] Trip trip)
        {
            if (id != trip.TripId) return NotFound();

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
            await PopulateDropdowns(trip.VehicleId, trip.DriverId);
            return View(trip);
        }

        // GET: Trips/Delete/5
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var trip = await _tripService.GetTripByIdAsync(id.Value);
            if (trip == null) return NotFound();
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

        // GET: Trips/UpdateStatus/5
        [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")]
        public async Task<IActionResult> UpdateStatus(int? id)
        {
            if (id == null) return NotFound();
            var trip = await _tripService.GetTripByIdAsync(id.Value);
            if (trip == null) return NotFound();

            if (User.IsInRole("Driver") && trip.DriverId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            {
                return Forbid();
            }

            ViewBag.StatusOptions = new SelectList(
                Enum.GetValues(typeof(TripStatus))
                    .Cast<TripStatus>()
                    .Select(s => new SelectListItem
                    {
                        Value = s.ToString(),
                        Text = s.GetDescription()
                    }), "Value", "Text", trip.Status);

            return View(trip);
        }

        // POST: Trips/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")]
        // Remove StartLocation, EndLocation, ScheduledStartTime, ScheduledEndTime, EstimatedDistanceKm, RouteDetails from Bind
        // as these are not being updated by this action and should retain original values from existingTrip.
        public async Task<IActionResult> UpdateStatus(int id, [Bind("TripId,Status,ActualStartTime,ActualEndTime,ActualDistanceKm")] Trip tripUpdate)
        {
            // Ensure the id from the route matches the TripId from the form
            if (id != tripUpdate.TripId)
            {
                _logger.LogWarning("TripId mismatch: Route ID {RouteId}, Form ID {FormId}", id, tripUpdate.TripId);
                return NotFound();
            }

            var existingTrip = await _tripService.GetTripByIdAsync(id); // Fetch the full existing trip
            if (existingTrip == null)
            {
                _logger.LogWarning("Trip with ID {TripId} not found for status update.", id);
                return NotFound();
            }

            // Authorization check: Only assigned driver or admin/operator can update
            if (User.IsInRole("Driver") && existingTrip.DriverId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            {
                _logger.LogWarning("Unauthorized attempt to update trip {TripId} by user {UserId}.", id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return Forbid();
            }

            // --- IMPORTANT: ONLY UPDATE THE FIELDS THAT ARE MEANT TO BE UPDATED ---
            existingTrip.Status = tripUpdate.Status;
            existingTrip.ActualStartTime = tripUpdate.ActualStartTime ?? existingTrip.ActualStartTime;
            existingTrip.ActualEndTime = tripUpdate.ActualEndTime ?? existingTrip.ActualEndTime;
            existingTrip.ActualDistanceKm = tripUpdate.ActualDistanceKm ?? existingTrip.ActualDistanceKm;

            // Automatic date/time stamping
            if (existingTrip.Status == TripStatus.InProgress.ToString() && !existingTrip.ActualStartTime.HasValue)
            {
                existingTrip.ActualStartTime = DateTime.Now;
            }
            else if (existingTrip.Status == TripStatus.Completed.ToString() && !existingTrip.ActualEndTime.HasValue)
            {
                existingTrip.ActualEndTime = DateTime.Now;
            }

            // Remove navigation properties from ModelState to prevent validation errors on FKs
            ModelState.Remove("Vehicle");
            ModelState.Remove("Driver");

            // DEBUGGING STEP: Log ModelState errors
            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogError("Model Error: {ErrorMessage}", error.ErrorMessage);
                        if (error.Exception != null)
                        {
                            _logger.LogError("Model Exception: {ExceptionMessage}", error.Exception.Message);
                        }
                    }
                }
            }
            // END DEBUGGING STEP


            if (ModelState.IsValid)
            {
                try
                {
                    await _tripService.UpdateTripAsync(existingTrip);
                    _logger.LogInformation("Trip {TripId} status updated successfully to {NewStatus}.", id, existingTrip.Status);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Concurrency conflict while updating trip {TripId}.", id);
                    if (!await _tripService.TripExistsAsync(existingTrip.TripId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred while updating trip {TripId}.", id);
                    // Consider adding a generic error view or message here
                    return StatusCode(500, "An internal server error occurred.");
                }
                return RedirectToAction(nameof(Details), new { id = existingTrip.TripId });
            }

            // If ModelState is invalid, repopulate ViewBag.StatusOptions and return the view with model
            ViewBag.StatusOptions = new SelectList(
                Enum.GetValues(typeof(TripStatus))
                    .Cast<TripStatus>()
                    .Select(s => new SelectListItem
                    {
                        Value = s.ToString(),
                        Text = s.GetDescription()
                    }), "Value", "Text", tripUpdate.Status);
            return View(tripUpdate); // Pass the tripUpdate back to the view to retain entered values
        }
    }
}
