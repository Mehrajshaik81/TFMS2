using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList
using Microsoft.EntityFrameworkCore; // For DbUpdateConcurrencyException
using TFMS.Data; // For ApplicationDbContext
using TFMS.Models;
using TFMS.Services; // For IMaintenanceService, IVehicleService
using System.Linq; // For .Where() and .ToList()

namespace TFMS.Controllers
{
    [Authorize] // All actions require authentication
    public class MaintenanceRecordsController : Controller
    {
        private readonly IMaintenanceService _maintenanceService;
        private readonly IVehicleService _vehicleService; // To get list of vehicles

        // Constructor injection
        public MaintenanceRecordsController(IMaintenanceService maintenanceService, IVehicleService vehicleService)
        {
            _maintenanceService = maintenanceService;
            _vehicleService = vehicleService;
        }

        // Helper method to populate Vehicle dropdowns
        private async Task PopulateDropdowns(int? selectedVehicle = null)
        {
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber", selectedVehicle);
        }

        // GET: MaintenanceRecords
        // Fleet Administrator and Fleet Operator can view maintenance records
        [Authorize(Roles = "Fleet Administrator,Fleet Operator")]
        public async Task<IActionResult> Index()
        {
            var maintenanceRecords = await _maintenanceService.GetAllMaintenanceRecordsAsync();
            return View(maintenanceRecords);
        }

        // GET: MaintenanceRecords/Details/5
        // Fleet Administrator and Fleet Operator can view details
        [Authorize(Roles = "Fleet Administrator,Fleet Operator")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenanceRecord = await _maintenanceService.GetMaintenanceByIdAsync(id.Value);
            if (maintenanceRecord == null)
            {
                return NotFound();
            }

            return View(maintenanceRecord);
        }

        // GET: MaintenanceRecords/Create
        // Only Fleet Administrator can create new maintenance records
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        // POST: MaintenanceRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Create([Bind("VehicleId,Description,ScheduledDate,Status,ActualCompletionDate,Cost,OdometerReadingKm,PerformedBy,MaintenanceType")] Maintenance maintenance)
        {
            // Remove navigation properties from ModelState
            ModelState.Remove("Vehicle");

            if (ModelState.IsValid)
            {
                await _maintenanceService.AddMaintenanceAsync(maintenance);
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdowns(maintenance.VehicleId);
            return View(maintenance);
        }

        // GET: MaintenanceRecords/Edit/5
        // Only Fleet Administrator can edit maintenance records
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenanceRecord = await _maintenanceService.GetMaintenanceByIdAsync(id.Value);
            if (maintenanceRecord == null)
            {
                return NotFound();
            }
            await PopulateDropdowns(maintenanceRecord.VehicleId);
            return View(maintenanceRecord);
        }

        // POST: MaintenanceRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("MaintenanceId,VehicleId,Description,ScheduledDate,Status,ActualCompletionDate,Cost,OdometerReadingKm,PerformedBy,MaintenanceType")] Maintenance maintenance)
        {
            if (id != maintenance.MaintenanceId)
            {
                return NotFound();
            }

            ModelState.Remove("Vehicle");

            if (ModelState.IsValid)
            {
                try
                {
                    await _maintenanceService.UpdateMaintenanceAsync(maintenance);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _maintenanceService.MaintenanceExistsAsync(maintenance.MaintenanceId))
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
            await PopulateDropdowns(maintenance.VehicleId);
            return View(maintenance);
        }

        // GET: MaintenanceRecords/Delete/5
        // Only Fleet Administrator can delete maintenance records
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenanceRecord = await _maintenanceService.GetMaintenanceByIdAsync(id.Value);
            if (maintenanceRecord == null)
            {
                return NotFound();
            }

            return View(maintenanceRecord);
        }

        // POST: MaintenanceRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _maintenanceService.DeleteMaintenanceAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}