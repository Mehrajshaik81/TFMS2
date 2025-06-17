// Controllers/VehiclesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TFMS.Data; // Ensure ApplicationDbContext is here
using TFMS.Models;
using TFMS.Services;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // For _logger

namespace TFMS.Controllers
{
    [Authorize(Roles = "Fleet Administrator")] // Only administrators can manage vehicles
    public class VehiclesController : Controller
    {
        private readonly ILogger<VehiclesController> _logger; // Declare logger
        private readonly IVehicleService _vehicleService;

        // Constructor injection
        public VehiclesController(ILogger<VehiclesController> logger, IVehicleService vehicleService) // Inject logger
        {
            _logger = logger; // Assign logger
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vehicles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
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

            // Check if the vehicle has associated trips before showing the delete confirmation
            var hasTrips = await _vehicleService.HasAssociatedTripsAsync(id.Value);
            if (hasTrips)
            {
                // Add a message to ViewBag to display in the view
                ViewBag.DeleteError = "This vehicle cannot be deleted because it has associated trips. Please delete or reassign its trips first.";
                _logger.LogWarning("Attempted to delete vehicle {VehicleId} with associated trips.", id.Value);
            }

            return View(vehicle);
        }

        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Re-check for associated trips just before deletion to prevent database error
            var hasTrips = await _vehicleService.HasAssociatedTripsAsync(id);
            if (hasTrips)
            {
                // Redirect back to Delete GET action with error message
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
                // Optionally add a more general error message if other issues occur
                TempData["DeleteError"] = "An error occurred while trying to delete the vehicle.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
