// Controllers/VehiclesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TFMS.Models;
using TFMS.Services;
using Microsoft.EntityFrameworkCore; // Needed for DbUpdateConcurrencyException

namespace TFMS.Controllers
{
    // This attribute ensures that only authenticated users can access this controller.
    // We will refine roles later.
    [Authorize]
    public class VehiclesController : Controller
    {
        private readonly IVehicleService _vehicleService;

        // Constructor injection: ASP.NET Core DI provides an instance of IVehicleService
        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        // GET: Vehicles
        // Allows Fleet Administrators and Fleet Operators to view the list of vehicles
        [Authorize(Roles = "Fleet Administrator,Fleet Operator")]
        public async Task<IActionResult> Index()
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            return View(vehicles);
        }

        // GET: Vehicles/Details/5
        // Allows Fleet Administrators and Fleet Operators to view details of a specific vehicle
        [Authorize(Roles = "Fleet Administrator,Fleet Operator")]
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
        // Only Fleet Administrators can create new vehicles
        [Authorize(Roles = "Fleet Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vehicles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // Only Fleet Administrators can create new vehicles
        [HttpPost]
        [ValidateAntiForgeryToken] // Prevents Cross-Site Request Forgery attacks
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Create([Bind("RegistrationNumber,Capacity,Status,LastServicedDate,Make,Model,ManufacturingYear,FuelType,CurrentOdometerKm")] Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                await _vehicleService.AddVehicleAsync(vehicle);
                return RedirectToAction(nameof(Index));
            }
            return View(vehicle);
        }

        // GET: Vehicles/Edit/5
        // Only Fleet Administrators can edit vehicle details
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // Only Fleet Administrators can edit vehicle details
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
                        throw; // Re-throw if it's a different concurrency issue
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(vehicle);
        }

        // GET: Vehicles/Delete/5
        // Only Fleet Administrators can delete vehicles
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
        // Only Fleet Administrators can delete vehicles
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