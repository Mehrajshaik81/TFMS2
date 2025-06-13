// Controllers/FuelRecordsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList
using Microsoft.EntityFrameworkCore; // For DbUpdateConcurrencyException
using TFMS.Data;
using TFMS.Models;
using TFMS.Services;
// For IFuelService, IVehicleService new service

namespace TFMS.Controllers
{
    [Authorize] // All actions require authentication
    public class FuelRecordsController : Controller
    {
        private readonly IFuelService _fuelService;
        private readonly IVehicleService _vehicleService; // To get list of vehicles
        private readonly ApplicationDbContext _context; // For getting drivers

        // Constructor injection
        public FuelRecordsController(IFuelService fuelService, IVehicleService vehicleService, ApplicationDbContext context)
        {
            _fuelService = fuelService;
            _vehicleService = vehicleService;
            _context = context;
        }

        // Helper method to populate Vehicle and Driver dropdowns
        private async Task PopulateDropdowns(int? selectedVehicle = null, string? selectedDriver = null)
        {
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber", selectedVehicle);

            // Get all users who are marked as active drivers for the dropdown
            var drivers = await _context.Users
                                        .Where(u => u.IsActiveDriver)
                                        .ToListAsync();
            ViewBag.DriverId = new SelectList(drivers, "Id", "Email", selectedDriver); // Using Id for value, Email for display
        }

        // GET: FuelRecords
        // Fleet Administrator, Fleet Operator, and Driver can view fuel records (Driver sees their own)
        [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")]
        public async Task<IActionResult> Index()
        {
            // If the user is a Driver, only show their own fuel records
            if (User.IsInRole("Driver"))
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Forbid(); // Should not happen if authenticated, but good safeguard
                }
                var driverFuelRecords = await _fuelService.GetAllFuelRecordsAsync();
                return View(driverFuelRecords.Where(f => f.DriverId == userId).ToList());
            }
            // Otherwise, show all fuel records for Fleet Admin/Operator
            else
            {
                var allFuelRecords = await _fuelService.GetAllFuelRecordsAsync();
                return View(allFuelRecords);
            }
        }

        // GET: FuelRecords/Details/5
        // Fleet Administrator, Fleet Operator, and Driver can view fuel record details
        [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fuelRecord = await _fuelService.GetFuelRecordByIdAsync(id.Value);
            if (fuelRecord == null)
            {
                return NotFound();
            }

            // If user is a Driver, ensure they can only view their own record
            if (User.IsInRole("Driver") && fuelRecord.DriverId != User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value)
            {
                return Forbid(); // Driver trying to view another driver's record
            }

            return View(fuelRecord);
        }

        // GET: FuelRecords/Create
        // Fleet Administrator and Driver can create fuel records
        [Authorize(Roles = "Fleet Administrator,Driver")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();

            // Pre-select current user as driver if they are a driver
            if (User.IsInRole("Driver"))
            {
                ViewBag.DriverId = new SelectList(
                    new List<ApplicationUser> { await _context.Users.FindAsync(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value) },
                    "Id", "Email", User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                );
            }
            return View();
        }

        // POST: FuelRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator,Driver")]
        public async Task<IActionResult> Create([Bind("VehicleId,Date,FuelQuantity,Cost,OdometerReadingKm,Location,DriverId")] FuelRecord fuelRecord)
        {
            // If the user is a driver, force their DriverId to prevent them from creating records for others
            if (User.IsInRole("Driver"))
            {
                fuelRecord.DriverId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }

            // Remove navigation properties from ModelState to prevent validation errors on FKs
            ModelState.Remove("Vehicle");
            ModelState.Remove("Driver");

            if (ModelState.IsValid)
            {
                await _fuelService.AddFuelRecordAsync(fuelRecord);
                return RedirectToAction(nameof(Index));
            }

            // If ModelState is invalid, repopulate dropdowns
            await PopulateDropdowns(fuelRecord.VehicleId, fuelRecord.DriverId);
            return View(fuelRecord);
        }

        // GET: FuelRecords/Edit/5
        // Only Fleet Administrator can edit fuel records (drivers can only create)
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fuelRecord = await _fuelService.GetFuelRecordByIdAsync(id.Value);
            if (fuelRecord == null)
            {
                return NotFound();
            }

            await PopulateDropdowns(fuelRecord.VehicleId, fuelRecord.DriverId);
            return View(fuelRecord);
        }

        // POST: FuelRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("FuelId,VehicleId,Date,FuelQuantity,Cost,OdometerReadingKm,Location,DriverId")] FuelRecord fuelRecord)
        {
            if (id != fuelRecord.FuelId)
            {
                return NotFound();
            }

            ModelState.Remove("Vehicle");
            ModelState.Remove("Driver");

            if (ModelState.IsValid)
            {
                try
                {
                    await _fuelService.UpdateFuelRecordAsync(fuelRecord);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _fuelService.FuelRecordExistsAsync(fuelRecord.FuelId))
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
            await PopulateDropdowns(fuelRecord.VehicleId, fuelRecord.DriverId);
            return View(fuelRecord);
        }

        // GET: FuelRecords/Delete/5
        // Only Fleet Administrator can delete fuel records
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fuelRecord = await _fuelService.GetFuelRecordByIdAsync(id.Value);
            if (fuelRecord == null)
            {
                return NotFound();
            }

            return View(fuelRecord);
        }

        // POST: FuelRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _fuelService.DeleteFuelRecordAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}