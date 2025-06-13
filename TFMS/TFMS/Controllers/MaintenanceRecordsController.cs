// Controllers/MaintenanceRecordsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList
using Microsoft.EntityFrameworkCore;
using TFMS.Models;
using TFMS.Services;
using System.Linq;
using System.Collections.Generic; // For List

namespace TFMS.Controllers
{
    [Authorize(Roles = "Fleet Administrator,Fleet Operator")]
    public class MaintenanceRecordsController : Controller
    {
        private readonly IMaintenanceService _maintenanceService;
        private readonly IVehicleService _vehicleService;

        public MaintenanceRecordsController(IMaintenanceService maintenanceService,
                                            IVehicleService vehicleService)
        {
            _maintenanceService = maintenanceService;
            _vehicleService = vehicleService;
        }

        // GET: MaintenanceRecords
        public async Task<IActionResult> Index(string? searchString, string? statusFilter, int? vehicleIdFilter, string? maintenanceTypeFilter)
        {
            // Store current filter values in ViewBag to persist them in the UI
            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentStatusFilter = statusFilter;
            ViewBag.CurrentVehicleFilter = vehicleIdFilter;
            ViewBag.CurrentMaintenanceTypeFilter = maintenanceTypeFilter;

            // Prepare Status filter options
            var statusOptions = new List<SelectListItem> // Use SelectListItem directly
            {
                new SelectListItem { Value = "All", Text = "All" },
                new SelectListItem { Value = "Scheduled", Text = "Scheduled" },
                new SelectListItem { Value = "In Progress", Text = "In Progress" },
                new SelectListItem { Value = "Completed", Text = "Completed" },
                new SelectListItem { Value = "Overdue", Text = "Overdue" },
                new SelectListItem { Value = "Cancelled", Text = "Cancelled" }
            };
            // FIX: Pass the collection of SelectListItems, then the selected value
            ViewBag.StatusFilter = new SelectList(statusOptions, "Value", "Text", statusFilter);

            // Prepare Vehicle filter options
            var allVehicles = await _vehicleService.GetAllVehiclesAsync();
            var vehicleListItems = allVehicles.Select(v => new SelectListItem { Value = v.VehicleId.ToString(), Text = v.RegistrationNumber }).ToList();
            vehicleListItems.Insert(0, new SelectListItem { Value = "0", Text = "All Vehicles" });
            // FIX: Specify dataValueField and dataTextField
            ViewBag.VehicleFilter = new SelectList(vehicleListItems, "Value", "Text", vehicleIdFilter.ToString());

            // Prepare Maintenance Type filter options
            var allMaintenanceRecordsForFilters = await _maintenanceService.GetAllMaintenanceRecordsAsync();
            var maintenanceTypeOptions = allMaintenanceRecordsForFilters.Select(m => m.MaintenanceType).Distinct().ToList();
            var maintenanceTypeListItems = maintenanceTypeOptions.Select(type => new SelectListItem { Value = type, Text = type }).ToList();
            maintenanceTypeListItems.Insert(0, new SelectListItem { Value = "All", Text = "All" });
            // FIX: Specify dataValueField and dataTextField
            ViewBag.MaintenanceTypeFilter = new SelectList(maintenanceTypeListItems, "Value", "Text", maintenanceTypeFilter);

            // Fetch maintenance records based on filters
            var maintenanceRecords = await _maintenanceService.GetAllMaintenanceRecordsAsync(searchString, statusFilter, vehicleIdFilter, maintenanceTypeFilter);
            return View(maintenanceRecords);
        }

        // GET: MaintenanceRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenance = await _maintenanceService.GetMaintenanceRecordByIdAsync(id.Value);
            if (maintenance == null)
            {
                return NotFound();
            }

            return View(maintenance);
        }

        // GET: MaintenanceRecords/Create
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Create()
        {
            // FIX: Specify dataValueField and dataTextField
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber");
            return View();
        }

        // POST: MaintenanceRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Create([Bind("MaintenanceId,VehicleId,Description,ScheduledDate,Status,ActualCompletionDate,Cost,OdometerReadingKm,PerformedBy,MaintenanceType")] Maintenance maintenance)
        {
            if (ModelState.IsValid)
            {
                // Ensure status defaults to "Scheduled" if not explicitly set in the form
                if (string.IsNullOrEmpty(maintenance.Status))
                {
                    maintenance.Status = "Scheduled";
                }
                await _maintenanceService.AddMaintenanceRecordAsync(maintenance);
                return RedirectToAction(nameof(Index));
            }
            // FIX: Specify dataValueField and dataTextField
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber", maintenance.VehicleId);
            return View(maintenance);
        }

        // GET: MaintenanceRecords/Edit/5
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenance = await _maintenanceService.GetMaintenanceRecordByIdAsync(id.Value);
            if (maintenance == null)
            {
                return NotFound();
            }
            // FIX: Specify dataValueField and dataTextField
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber", maintenance.VehicleId);
            return View(maintenance);
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

            if (ModelState.IsValid)
            {
                try
                {
                    await _maintenanceService.UpdateMaintenanceRecordAsync(maintenance);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _maintenanceService.MaintenanceRecordExistsAsync(maintenance.MaintenanceId))
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
            // FIX: Specify dataValueField and dataTextField
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber", maintenance.VehicleId);
            return View(maintenance);
        }

        // GET: MaintenanceRecords/Delete/5
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenance = await _maintenanceService.GetMaintenanceRecordByIdAsync(id.Value);
            if (maintenance == null)
            {
                return NotFound();
            }

            return View(maintenance);
        }

        // POST: MaintenanceRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _maintenanceService.DeleteMaintenanceRecordAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
