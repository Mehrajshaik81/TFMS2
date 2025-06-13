using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TFMS.Data;
using TFMS.Models;
using TFMS.Services;

namespace TFMS.Controllers
{
    [Authorize]
    public class FuelRecordsController : Controller
    {
        private readonly IFuelService _fuelService;
        private readonly IVehicleService _vehicleService;
        private readonly ApplicationDbContext _context;

        public FuelRecordsController(IFuelService fuelService, IVehicleService vehicleService, ApplicationDbContext context)
        {
            _fuelService = fuelService;
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

        [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (User.IsInRole("Driver"))
            {
                var driverFuelRecords = await _fuelService.GetAllFuelRecordsAsync();
                return View(driverFuelRecords.Where(f => f.DriverId == userId).ToList());
            }

            var allFuelRecords = await _fuelService.GetAllFuelRecordsAsync();
            return View(allFuelRecords);
        }

        [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var fuelRecord = await _fuelService.GetFuelRecordByIdAsync(id.Value);
            if (fuelRecord == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (User.IsInRole("Driver") && fuelRecord.DriverId != userId)
            {
                return Forbid();
            }

            return View(fuelRecord);
        }

        [Authorize(Roles = "Fleet Administrator,Driver")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();

            if (User.IsInRole("Driver"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var driver = await _context.Users.FindAsync(userId);

                ViewBag.DriverId = new SelectList(new List<ApplicationUser> { driver }, "Id", "Email", userId);
                ViewBag.DriverIdValue = userId;
                ViewBag.DriverName = driver.Email;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator,Driver")]
        public async Task<IActionResult> Create([Bind("VehicleId,Date,FuelQuantity,Cost,OdometerReadingKm,Location,DriverId")] FuelRecord fuelRecord)
        {
            if (User.IsInRole("Driver"))
            {
                fuelRecord.DriverId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            ModelState.Remove("Vehicle");
            ModelState.Remove("Driver");

            if (ModelState.IsValid)
            {
                await _fuelService.AddFuelRecordAsync(fuelRecord);
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(fuelRecord.VehicleId, fuelRecord.DriverId);
            return View(fuelRecord);
        }

        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var fuelRecord = await _fuelService.GetFuelRecordByIdAsync(id.Value);
            if (fuelRecord == null) return NotFound();

            await PopulateDropdowns(fuelRecord.VehicleId, fuelRecord.DriverId);
            return View(fuelRecord);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("FuelId,VehicleId,Date,FuelQuantity,Cost,OdometerReadingKm,Location,DriverId")] FuelRecord fuelRecord)
        {
            if (id != fuelRecord.FuelId) return NotFound();

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
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(fuelRecord.VehicleId, fuelRecord.DriverId);
            return View(fuelRecord);
        }

        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var fuelRecord = await _fuelService.GetFuelRecordByIdAsync(id.Value);
            if (fuelRecord == null) return NotFound();

            return View(fuelRecord);
        }

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
