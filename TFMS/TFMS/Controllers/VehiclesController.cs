// Controllers/VehiclesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TFMS.Data;
using TFMS.Models;
using TFMS.Services;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TFMS.Controllers
{
    // Allow both Fleet Administrator and Fleet Operator to manage vehicles
    [Authorize(Roles = "Fleet Administrator,Fleet Operator")]
    public class VehiclesController : Controller
    {
        private readonly ILogger<VehiclesController> _logger;
        private readonly IVehicleService _vehicleService;

        // Constructor injection
        public VehiclesController(ILogger<VehiclesController> logger, IVehicleService vehicleService)
        {
            _logger = logger;
            _vehicleService = vehicleService;
        }

        // GET: Vehicles
        public async Task<IActionResult> Index()
        {
            return View(await _vehicleService.GetAllVehiclesAsync());
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
        // Only Fleet Administrator can create vehicles (more restrictive than general access)
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
        // Only Fleet Administrator can edit vehicles
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
        // Only Fleet Administrator can delete vehicles
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

            var hasTrips = await _vehicleService.HasAssociatedTripsAsync(id.Value);
            if (hasTrips)
            {
                ViewBag.DeleteError = "This vehicle cannot be deleted because it has associated trips. Please delete or reassign its trips first.";
                _logger.LogWarning("Attempted to delete vehicle {VehicleId} with associated trips.", id.Value);
            }

            return View(vehicle);
        }

        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasTrips = await _vehicleService.HasAssociatedTripsAsync(id);
            if (hasTrips)
            {
                TempData["DeleteError"] = "This vehicle cannot be deleted because it has associated trips. Please delete or reassign its trips first.";
                _logger.LogWarning("Prevented deletion of vehicle {VehicleId} due to associated trips.", id);
                return RedirectToAction(nameof(Delete), new { id = id });
            }

            try
            {
                await _vehicleService.DeleteVehicleAsync(id);
                _logger.LogInformation("Vehicle {VehicleId} deleted successfully.", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle {VehicleId}.", id);
                TempData["DeleteError"] = "An error occurred while trying to delete the vehicle.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
