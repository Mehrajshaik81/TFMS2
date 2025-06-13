// Controllers/VehiclesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList
using Microsoft.EntityFrameworkCore;
using TFMS.Models;
using TFMS.Services;
using System.Linq; // For Distinct()

namespace TFMS.Controllers
{
    [Authorize(Roles = "Fleet Administrator,Fleet Operator")]
    public class VehiclesController : Controller
    {
        private readonly IVehicleService _vehicleService;

        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        // GET: Vehicles
        public async Task<IActionResult> Index(string? searchString, string? statusFilter, string? fuelTypeFilter)
        {
            // --- ADD THESE LINES HERE ---
            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentStatusFilter = statusFilter;
            ViewBag.CurrentFuelTypeFilter = fuelTypeFilter;
            // ---------------------------

            // Get all unique statuses and fuel types for filter dropdowns
            // We fetch all vehicles *first* to ensure the dropdowns contain all possible options
            var allVehiclesForFilters = await _vehicleService.GetAllVehiclesAsync();
            ViewBag.StatusFilter = new SelectList(
                allVehiclesForFilters.Select(v => v.Status).Distinct().Prepend("All").ToList(), // Add "All" option
                statusFilter // Select current filter value
            );
            ViewBag.FuelTypeFilter = new SelectList(
                allVehiclesForFilters.Select(v => v.FuelType).Distinct().Prepend("All").ToList(), // Add "All" option
                fuelTypeFilter // Select current filter value
            );

            // Now get the vehicles based on the applied filters
            var vehicles = await _vehicleService.GetAllVehiclesAsync(searchString, statusFilter, fuelTypeFilter);
            return View(vehicles);
        }

        // GET: Vehicles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _vehicleService.GetVehicleByIdAsync(id.Value);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // GET: Vehicles/Create
        [Authorize(Roles = "Fleet Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vehicles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Create([Bind("VehicleId,RegistrationNumber,Capacity,Status,LastServicedDate,Make,Model,ManufacturingYear,FuelType,CurrentOdometerKm")] Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                await _vehicleService.AddVehicleAsync(vehicle);
                return RedirectToAction(nameof(Index));
            }
            return View(vehicle);
        }

        // GET: Vehicles/Edit/5
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _vehicleService.GetVehicleByIdAsync(id.Value);
            if (vehicle == null)
            {
                return NotFound();
            }
            return View(vehicle);
        }

        // POST: Vehicles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("VehicleId,RegistrationNumber,Capacity,Status,LastServicedDate,Make,Model,ManufacturingYear,FuelType,CurrentOdometerKm")] Vehicle vehicle)
        {
            if (id != vehicle.VehicleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _vehicleService.UpdateVehicleAsync(vehicle);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _vehicleService.VehicleExistsAsync(vehicle.VehicleId))
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
            return View(vehicle);
        }

        // GET: Vehicles/Delete/5
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _vehicleService.GetVehicleByIdAsync(id.Value);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _vehicleService.DeleteVehicleAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
