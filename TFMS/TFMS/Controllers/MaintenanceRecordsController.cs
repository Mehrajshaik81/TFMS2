// Controllers/MaintenanceRecordsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TFMS.Models; // Ensure correct namespace, includes MaintenanceStatus enum and EnumExtensions
using TFMS.Services;
using System.Linq;
using System.Collections.Generic;
using System; // For Enum

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
            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentStatusFilter = statusFilter;
            ViewBag.CurrentVehicleFilter = vehicleIdFilter;
            ViewBag.CurrentMaintenanceTypeFilter = maintenanceTypeFilter;

            // Prepare Status filter options using enum descriptions
            var statusOptions = new List<SelectListItem>();
            statusOptions.Add(new SelectListItem { Value = "All", Text = "All" });
            foreach (MaintenanceStatus statusEnum in Enum.GetValues(typeof(MaintenanceStatus)))
            {
                statusOptions.Add(new SelectListItem { Value = statusEnum.GetDescription(), Text = statusEnum.GetDescription() }); // Use GetDescription()
            }
            ViewBag.StatusFilter = new SelectList(statusOptions, "Value", "Text", statusFilter);

            // Prepare Vehicle filter options
            var allVehicles = await _vehicleService.GetAllVehiclesAsync();
            var vehicleListItems = allVehicles.Select(v => new SelectListItem { Value = v.VehicleId.ToString(), Text = v.RegistrationNumber }).ToList();
            vehicleListItems.Insert(0, new SelectListItem { Value = "0", Text = "All Vehicles" });
            ViewBag.VehicleFilter = new SelectList(vehicleListItems, "Value", "Text", vehicleIdFilter.ToString());

            // Prepare Maintenance Type filter options
            var allMaintenanceRecordsForFilters = await _maintenanceService.GetAllMaintenanceRecordsAsync(null, null, null, null); // Fetch all for distinct types
            var maintenanceTypeOptions = allMaintenanceRecordsForFilters.Select(m => m.MaintenanceType).Distinct().ToList();
            var maintenanceTypeListItems = maintenanceTypeOptions.Where(t => !string.IsNullOrEmpty(t))
                                                               .Select(type => new SelectListItem { Value = type, Text = type }).ToList();
            maintenanceTypeListItems.Insert(0, new SelectListItem { Value = "All", Text = "All" });
            ViewBag.MaintenanceTypeFilter = new SelectList(maintenanceTypeListItems, "Value", "Text", maintenanceTypeFilter);

            var maintenanceRecords = await _maintenanceService.GetAllMaintenanceRecordsAsync(searchString, statusFilter, vehicleIdFilter, maintenanceTypeFilter);
            return View(maintenanceRecords);
        }

        // GET: MaintenanceRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var maintenance = await _maintenanceService.GetMaintenanceRecordByIdAsync(id.Value);
            if (maintenance == null) return NotFound();
            return View(maintenance);
        }

        // GET: MaintenanceRecords/Create
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Create()
        {
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber");

            // Prepare status dropdown for Create view using enum descriptions
            var statusListForForm = new List<SelectListItem>();
            foreach (MaintenanceStatus statusEnum in Enum.GetValues(typeof(MaintenanceStatus)))
            {
                statusListForForm.Add(new SelectListItem { Value = statusEnum.ToString(), Text = statusEnum.GetDescription() }); // Value is Enum.Name, Text is Description
            }
            ViewBag.StatusOptions = new SelectList(statusListForForm, "Value", "Text", MaintenanceStatus.Scheduled.ToString());
            return View();
        }

        // POST: MaintenanceRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Create([Bind("MaintenanceId,VehicleId,Description,ScheduledDate,Status,ActualCompletionDate,Cost,OdometerReadingKm,PerformedBy,MaintenanceType")] Maintenance maintenance)
        {
            // Model binding will convert the string (enum name) from the form to MaintenanceStatus enum
            if (ModelState.IsValid)
            {
                await _maintenanceService.AddMaintenanceRecordAsync(maintenance);
                return RedirectToAction(nameof(Index));
            }
            // If model state is not valid, re-populate ViewBags
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber", maintenance.VehicleId);
            var statusListForForm = new List<SelectListItem>();
            foreach (MaintenanceStatus statusEnum in Enum.GetValues(typeof(MaintenanceStatus)))
            {
                statusListForForm.Add(new SelectListItem { Value = statusEnum.ToString(), Text = statusEnum.GetDescription() });
            }
            ViewBag.StatusOptions = new SelectList(statusListForForm, "Value", "Text", maintenance.Status.ToString());
            return View(maintenance);
        }

        // GET: MaintenanceRecords/Edit/5
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var maintenance = await _maintenanceService.GetMaintenanceRecordByIdAsync(id.Value);
            if (maintenance == null) return NotFound();

            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber", maintenance.VehicleId);

            // Prepare status dropdown for Edit view using enum descriptions
            var statusListForForm = new List<SelectListItem>();
            foreach (MaintenanceStatus statusEnum in Enum.GetValues(typeof(MaintenanceStatus)))
            {
                statusListForForm.Add(new SelectListItem { Value = statusEnum.ToString(), Text = statusEnum.GetDescription() });
            }
            ViewBag.StatusOptions = new SelectList(statusListForForm, "Value", "Text", maintenance.Status.ToString());
            return View(maintenance);
        }

        // POST: MaintenanceRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("MaintenanceId,VehicleId,Description,ScheduledDate,Status,ActualCompletionDate,Cost,OdometerReadingKm,PerformedBy,MaintenanceType")] Maintenance maintenance)
        {
            if (id != maintenance.MaintenanceId) return NotFound();

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
            // If model state is not valid, re-populate ViewBags
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber", maintenance.VehicleId);
            var statusListForForm = new List<SelectListItem>();
            foreach (MaintenanceStatus statusEnum in Enum.GetValues(typeof(MaintenanceStatus)))
            {
                statusListForForm.Add(new SelectListItem { Value = statusEnum.ToString(), Text = statusEnum.GetDescription() });
            }
            ViewBag.StatusOptions = new SelectList(statusListForForm, "Value", "Text", maintenance.Status.ToString());
            return View(maintenance);
        }

        // GET: MaintenanceRecords/Delete/5
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var maintenance = await _maintenanceService.GetMaintenanceRecordByIdAsync(id.Value);
            if (maintenance == null) return NotFound();
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