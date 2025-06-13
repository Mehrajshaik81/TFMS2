// Controllers/PerformanceReportsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // Added for UserManager (if not already there)
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList
using TFMS.Models; // Your Models namespace
using TFMS.Services; // Your Services namespace
using System.Security.Claims; // Required for ClaimTypes.NameIdentifier

namespace TFMS.Controllers
{
    [Authorize(Roles = "Fleet Administrator,Fleet Operator")]
    public class PerformanceReportsController : Controller
    {
        private readonly IPerformanceService _performanceService;
        private readonly IVehicleService _vehicleService; // <<< ADD THIS
        private readonly UserManager<ApplicationUser> _userManager; // Ensure this is present if used in other actions

        public PerformanceReportsController(IPerformanceService performanceService,
                                            IVehicleService vehicleService, // <<< ADD THIS
                                            UserManager<ApplicationUser> userManager) // Ensure this is present
        {
            _performanceService = performanceService;
            _vehicleService = vehicleService; // <<< ASSIGN THIS
            _userManager = userManager;       // Ensure this is assigned
        }

        // GET: PerformanceReports
        public async Task<IActionResult> Index()
        {
            var reports = await _performanceService.GetAllPerformanceReportsAsync();
            return View(reports);
        }

        // GET: PerformanceReports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _performanceService.GetPerformanceReportByIdAsync(id.Value);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // GET: PerformanceReports/Generate
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Generate() // <<< NOW ASYNC
        {
            // Populate ViewBag for Vehicle Filter dropdown
            var allVehicles = await _vehicleService.GetAllVehiclesAsync();
            var vehicleListItems = allVehicles.Select(v => new SelectListItem { Value = v.VehicleId.ToString(), Text = v.RegistrationNumber }).ToList();
            vehicleListItems.Insert(0, new SelectListItem { Value = "0", Text = "All Vehicles" }); // "0" means all vehicles
            ViewBag.Vehicles = new SelectList(vehicleListItems, "Value", "Text"); // <<< ADDED FOR DROPDOWN

            return View();
        }

        // POST: PerformanceReports/GenerateFuelEfficiencyReport
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> GenerateFuelEfficiencyReport(DateTime startDate, DateTime endDate, int? vehicleId) // <<< ADDED vehicleId
        {
            if (startDate > endDate)
            {
                ModelState.AddModelError("", "Start date cannot be after end date.");
                // Re-populate ViewBag if returning to view due to error
                ViewBag.Vehicles = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber");
                return View("Generate");
            }

            var report = await _performanceService.GenerateFuelEfficiencyReportAsync(startDate, endDate, vehicleId); // <<< PASS vehicleId
            report.GeneratedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _performanceService.AddPerformanceReportAsync(report); // Save with GeneratedByUserId

            TempData["ReportGeneratedMessage"] = $"Fuel Efficiency Report generated successfully (ID: {report.PerformanceId}).";
            return RedirectToAction(nameof(Details), new { id = report.PerformanceId });
        }

        // POST: PerformanceReports/GenerateVehicleUtilizationReport
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> GenerateVehicleUtilizationReport(DateTime startDate, DateTime endDate, int? vehicleId) // <<< ADDED vehicleId
        {
            if (startDate > endDate)
            {
                ModelState.AddModelError("", "Start date cannot be after end date.");
                // Re-populate ViewBag if returning to view due to error
                ViewBag.Vehicles = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber");
                return View("Generate");
            }
            var report = await _performanceService.GenerateVehicleUtilizationReportAsync(startDate, endDate, vehicleId); // <<< PASS vehicleId
            report.GeneratedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _performanceService.AddPerformanceReportAsync(report); // Save with GeneratedByUserId

            TempData["ReportGeneratedMessage"] = $"Vehicle Utilization Report generated successfully (ID: {report.PerformanceId}).";
            return RedirectToAction(nameof(Details), new { id = report.PerformanceId });
        }

        // POST: PerformanceReports/GenerateMaintenanceCostReport
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> GenerateMaintenanceCostReport(DateTime startDate, DateTime endDate, int? vehicleId) // <<< ADDED vehicleId
        {
            if (startDate > endDate)
            {
                ModelState.AddModelError("", "Start date cannot be after end date.");
                // Re-populate ViewBag if returning to view due to error
                ViewBag.Vehicles = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber");
                return View("Generate");
            }
            var report = await _performanceService.GenerateMaintenanceCostReportAsync(startDate, endDate, vehicleId); // <<< PASS vehicleId
            report.GeneratedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _performanceService.AddPerformanceReportAsync(report); // Save with GeneratedByUserId

            TempData["ReportGeneratedMessage"] = $"Maintenance Cost Report generated successfully (ID: {report.PerformanceId}).";
            return RedirectToAction(nameof(Details), new { id = report.PerformanceId });
        }

        // GET: PerformanceReports/Delete/5Add commentMore actions
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var report = await _performanceService.GetPerformanceReportByIdAsync(id.Value);
            if (report == null) return NotFound();

            return View(report);
        }


        // POST: PerformanceReports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _performanceService.DeletePerformanceReportAsync(id);
            TempData["ReportDeletedMessage"] = "Report deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}