// Controllers/TripsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // ADDED
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList
using Microsoft.EntityFrameworkCore;
using TFMS.Models;
using TFMS.Services;
using System.Linq;
using System.Security.Claims; // For ClaimTypes.NameIdentifier
using System.Collections.Generic; // For List

namespace TFMS.Controllers
{
    [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")]
    public class TripsController : Controller
    {
        private readonly ITripService _tripService;
        private readonly IVehicleService _vehicleService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TripsController(ITripService tripService,
                               IVehicleService vehicleService,
                               UserManager<ApplicationUser> userManager)
        {
            _tripService = tripService;
            _vehicleService = vehicleService;
            _userManager = userManager;
        }

        // GET: Trips
        // Added parameters for search string, status, driver, and vehicle filters
        public async Task<IActionResult> Index(string? searchString, string? statusFilter, string? driverIdFilter, int? vehicleIdFilter)
        {
            // Store current filter values in ViewBag to persist them in the UI
            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentStatusFilter = statusFilter;
            ViewBag.CurrentDriverFilter = driverIdFilter;
            ViewBag.CurrentVehicleFilter = vehicleIdFilter;

            // Prepare Status filter options (static list)
            var statusOptions = new List<string> { "All", "Pending", "In Progress", "Completed", "Delayed", "Canceled" };
            ViewBag.StatusFilter = new SelectList(statusOptions, statusFilter);

            // Prepare Driver filter options
            var allDrivers = await _userManager.GetUsersInRoleAsync("Driver");
            var driverListItems = allDrivers.Select(d => new SelectListItem { Value = d.Id, Text = d.Email }).ToList();
            driverListItems.Insert(0, new SelectListItem { Value = "All", Text = "All Drivers" }); // Add "All Drivers" option

            // FIX: Specify dataValueField and dataTextField for SelectList
            ViewBag.DriverFilter = new SelectList(driverListItems, "Value", "Text", driverIdFilter);


            // Prepare Vehicle filter options
            var allVehicles = await _vehicleService.GetAllVehiclesAsync();
            var vehicleListItems = allVehicles.Select(v => new SelectListItem { Value = v.VehicleId.ToString(), Text = v.RegistrationNumber }).ToList();
            vehicleListItems.Insert(0, new SelectListItem { Value = "0", Text = "All Vehicles" }); // Use "0" for "All" vehicles

            // FIX: Specify dataValueField and dataTextField for SelectList
            ViewBag.VehicleFilter = new SelectList(vehicleListItems, "Value", "Text", vehicleIdFilter.ToString());

            // Fetch trips based on filters
            var trips = await _tripService.GetAllTripsAsync(searchString, statusFilter, driverIdFilter, vehicleIdFilter);

            // For drivers, filter to only show their trips after other filters are applied
            if (User.IsInRole("Driver") && !User.IsInRole("Fleet Administrator") && !User.IsInRole("Fleet Operator"))
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId != null)
                {
                    trips = trips.Where(t => t.DriverId == currentUserId);
                }
            }

            return View(trips);
        }

        // GET: Trips/Details/5
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
        public async Task<IActionResult> Create()
        {
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber");

            // For Create, if current user is a driver, pre-select them and show only their ID
            if (User.IsInRole("Driver") && !User.IsInRole("Fleet Administrator") && !User.IsInRole("Fleet Operator"))
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUser = await _userManager.FindByIdAsync(currentUserId);
                // FIX: Specify dataValueField and dataTextField for SelectList
                ViewBag.DriverId = new SelectList(new List<ApplicationUser> { currentUser }, "Id", "Email", currentUserId);
            }
            else // For Admin/Operator, show all drivers
            {
                // FIX: Specify dataValueField and dataTextField for SelectList
                ViewBag.DriverId = new SelectList(await _userManager.GetUsersInRoleAsync("Driver"), "Id", "Email");
            }
            return View();
        }

        // POST: Trips/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TripId,VehicleId,DriverId,StartLocation,EndLocation,ScheduledStartTime,ScheduledEndTime,Status,ActualStartTime,ActualEndTime,EstimatedDistanceKm,ActualDistanceKm,RouteDetails")] Trip trip)
        {
            if (ModelState.IsValid)
            {
                // Ensure the Status is set to "Pending" if not explicitly handled by a hidden input or default
                if (string.IsNullOrEmpty(trip.Status))
                {
                    trip.Status = "Pending";
                }
                await _tripService.AddTripAsync(trip);
                return RedirectToAction(nameof(Index));
            }
            // If model state is not valid, re-populate ViewBags
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber", trip.VehicleId);

            if (User.IsInRole("Driver") && !User.IsInRole("Fleet Administrator") && !User.IsInRole("Fleet Operator"))
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUser = await _userManager.FindByIdAsync(currentUserId);
                ViewBag.DriverId = new SelectList(new List<ApplicationUser> { currentUser }, "Id", "Email", currentUserId);
            }
            else
            {
                ViewBag.DriverId = new SelectList(await _userManager.GetUsersInRoleAsync("Driver"), "Id", "Email", trip.DriverId);
            }

            return View(trip);
        }

        // GET: Trips/Edit/5
        [Authorize(Roles = "Fleet Administrator,Fleet Operator")] // <<< Ensure this authorization is correct
        public async Task<IActionResult> Edit(int? id)
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

            // Drivers can only update status, not full edit.
            // If a driver tries to access this, they should be forbidden or redirected.
            if (User.IsInRole("Driver"))
            {
                // This check is already done by [Authorize], but adding it for clarity
                // If a Driver *should* edit, you'd change the Authorize attribute and add logic here.
                // Assuming only Admin/Operator can fully edit a trip.
                return Forbid(); // Deny access for Driver role to full edit.
            }

            // FIX: Specify dataValueField and dataTextField for SelectList for VehicleId
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber", trip.VehicleId);
            // FIX: Specify dataValueField and dataTextField for SelectList for DriverId
            ViewBag.DriverId = new SelectList(await _userManager.GetUsersInRoleAsync("Driver"), "Id", "Email", trip.DriverId);
            return View(trip);
        }

        // POST: Trips/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator,Fleet Operator")] // <<< Ensure this authorization is correct
        public async Task<IActionResult> Edit(int id, [Bind("TripId,VehicleId,DriverId,StartLocation,EndLocation,ScheduledStartTime,ScheduledEndTime,Status,ActualStartTime,ActualEndTime,EstimatedDistanceKm,ActualDistanceKm,RouteDetails")] Trip trip)
        {
            if (id != trip.TripId)
            {
                return NotFound();
            }

            // Re-fetch original trip details to preserve fields not in the Bind attribute if necessary
            // (e.g., if you only bind a subset of fields for security)
            var originalTrip = await _tripService.GetTripByIdAsync(id);
            if (originalTrip == null)
            {
                return NotFound();
            }

            // Update only the properties that are allowed to be edited via the form
            // Ensure you are not over-binding if some fields should only be set by other means (e.g., Status by UpdateStatus)
            originalTrip.VehicleId = trip.VehicleId;
            originalTrip.DriverId = trip.DriverId;
            originalTrip.StartLocation = trip.StartLocation;
            originalTrip.EndLocation = trip.EndLocation;
            originalTrip.ScheduledStartTime = trip.ScheduledStartTime;
            originalTrip.ScheduledEndTime = trip.ScheduledEndTime;
            // originalTrip.Status = trip.Status; // Be careful with this if UpdateStatus is separate
            originalTrip.EstimatedDistanceKm = trip.EstimatedDistanceKm;
            originalTrip.ActualDistanceKm = trip.ActualDistanceKm;
            originalTrip.ActualStartTime = trip.ActualStartTime;
            originalTrip.ActualEndTime = trip.ActualEndTime;
            originalTrip.RouteDetails = trip.RouteDetails;


            if (ModelState.IsValid)
            {
                try
                {
                    // If you only want to update selected fields, use originalTrip, not trip directly in UpdateAsync
                    await _tripService.UpdateTripAsync(originalTrip);
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
            // If model state is not valid, re-populate ViewBags
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber", trip.VehicleId);
            ViewBag.DriverId = new SelectList(await _userManager.GetUsersInRoleAsync("Driver"), "Id", "Email", trip.DriverId);
            return View(trip);
        }


        // GET: Trips/UpdateStatus/5
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

            // Drivers can only update status for their own trips
            if (User.IsInRole("Driver") && trip.DriverId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            {
                return Forbid(); // Or RedirectToAction("AccessDenied", "Account");
            }

            return View(trip);
        }

        // POST: Trips/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")]
        public async Task<IActionResult> UpdateStatus(int id, [Bind("TripId,VehicleId,DriverId,StartLocation,EndLocation,ScheduledStartTime,ScheduledEndTime,EstimatedDistanceKm,RouteDetails,Status,ActualStartTime,ActualEndTime,ActualDistanceKm")] Trip trip)
        {
            if (id != trip.TripId)
            {
                return NotFound();
            }

            // Fetch the original trip to prevent overposting issues and ensure driver can only update status/actual times
            var originalTrip = await _tripService.GetTripByIdAsync(id);
            if (originalTrip == null)
            {
                return NotFound();
            }

            // For drivers, ensure they are updating their own trip
            if (User.IsInRole("Driver") && originalTrip.DriverId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update only the allowed fields in originalTrip.
                    // This is crucial to prevent potentially malicious over-binding.
                    originalTrip.Status = trip.Status;
                    originalTrip.ActualStartTime = trip.ActualStartTime;
                    originalTrip.ActualEndTime = trip.ActualEndTime;
                    originalTrip.ActualDistanceKm = trip.ActualDistanceKm;

                    await _tripService.UpdateTripAsync(originalTrip);
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
            return View(trip); // If ModelState is not valid, re-render the view with errors
        }

        // GET: Trips/Delete/5
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
    }
}
