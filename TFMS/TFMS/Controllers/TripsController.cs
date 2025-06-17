using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TFMS.Data;
using TFMS.Models;
using TFMS.Services;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

// Alias to resolve ambiguity
using EnumHelper = TFMS.Models.EnumExtensions;

namespace TFMS.Controllers
{
    [Authorize]
    public class TripsController : Controller
    {
        private readonly ITripService _tripService;
        private readonly IVehicleService _vehicleService;
        private readonly ApplicationDbContext _context;

        public TripsController(ITripService tripService, IVehicleService vehicleService, ApplicationDbContext context)
        {
            _tripService = tripService;
            _vehicleService = vehicleService;
            _context = context;
        }

        private async Task PopulateDropdowns(int? selectedVehicle = null, string? selectedDriver = null)
        {
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber", selectedVehicle);

            var drivers = await _context.Users
                                        .Where(u => u.IsActiveDriver)
                                        .ToListAsync();
            ViewBag.DriverId = new SelectList(drivers, "Id", "Email", selectedDriver);
        }

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
                statusOptions.Add(new SelectListItem
                {
                    Value = statusEnum.ToString(),
                    Text = EnumHelper.GetDescription(statusEnum)
                });
            }
            ViewBag.StatusFilter = new SelectList(statusOptions, "Value", "Text", statusFilter);

            var allVehicles = await _vehicleService.GetAllVehiclesAsync();
            var vehicleListItems = allVehicles.Select(v => new SelectListItem
            {
                Value = v.VehicleId.ToString(),
                Text = v.RegistrationNumber
            }).ToList();
            vehicleListItems.Insert(0, new SelectListItem { Value = "0", Text = "All Vehicles" });
            ViewBag.VehicleFilter = new SelectList(vehicleListItems, "Value", "Text", vehicleIdFilter?.ToString());

            var allDrivers = await _context.Users.Where(u => u.IsActiveDriver).ToListAsync();
            var driverListItems = allDrivers.Select(d => new SelectListItem
            {
                Value = d.Id,
                Text = d.Email
            }).ToList();
            driverListItems.Insert(0, new SelectListItem { Value = "0", Text = "All Drivers" });
            ViewBag.DriverFilter = new SelectList(driverListItems, "Value", "Text", driverIdFilter);

            var trips = await _tripService.GetAllTripsAsync(searchString, statusFilter, vehicleIdFilter, driverIdFilter);
            return View(trips);
        }

        [Authorize(Roles = "Fleet Administrator,Fleet Operator")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var trip = await _tripService.GetTripByIdAsync(id.Value);
            if (trip == null) return NotFound();
            return View(trip);
        }

        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

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

        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var trip = await _tripService.GetTripByIdAsync(id.Value);
            if (trip == null) return NotFound();

            await PopulateDropdowns(trip.VehicleId, trip.DriverId);
            return View(trip);
        }

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
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    return View(trip);
                }
            }

            await PopulateDropdowns(trip.VehicleId, trip.DriverId);
            return View(trip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")]
        public async Task<IActionResult> UpdateStatus(int id, [Bind("TripId,Status,ActualStartTime,ActualEndTime,ActualDistanceKm")] Trip tripUpdate)
        {
            if (id != tripUpdate.TripId) return NotFound();

            var existingTrip = await _tripService.GetTripByIdAsync(id);
            if (existingTrip == null) return NotFound();

            if (User.IsInRole("Driver") && existingTrip.DriverId != User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value)
            {
                return Forbid();
            }

            existingTrip.Status = tripUpdate.Status;
            existingTrip.ActualStartTime = tripUpdate.ActualStartTime;
            existingTrip.ActualEndTime = tripUpdate.ActualEndTime;
            existingTrip.ActualDistanceKm = tripUpdate.ActualDistanceKm;

            await _tripService.UpdateTripAsync(existingTrip);
            return RedirectToAction(nameof(Index));
        }
    }
}
