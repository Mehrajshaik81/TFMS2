using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TFMS.Models;
using TFMS.Services;
using System.Threading.Tasks;
using System.Security.Claims;

namespace TFMS.Controllers
{
    [Authorize(Roles = "Fleet Administrator,Fleet Operator")]
    public class PerformanceReportsController : Controller
    {
        private readonly IPerformanceService _performanceService;

        public PerformanceReportsController(IPerformanceService performanceService)
        {
            _performanceService = performanceService;
        }

        public async Task<IActionResult> Index()
        {
            var reports = await _performanceService.GetAllPerformanceReportsAsync();
            return View(reports);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var report = await _performanceService.GetPerformanceReportByIdAsync(id.Value);
            if (report == null) return NotFound();

            return View(report);
        }

        [Authorize(Roles = "Fleet Administrator")]
        public IActionResult Generate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> GenerateFuelEfficiencyReport(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                ModelState.AddModelError("", "Start date cannot be after end date.");
                return View("Generate");
            }

            var report = await _performanceService.GenerateFuelEfficiencyReportAsync(startDate, endDate);
            report.GeneratedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            TempData["ReportGeneratedMessage"] = $"Fuel Efficiency Report generated successfully (ID: {report.PerformanceId}).";
            return RedirectToAction(nameof(Details), new { id = report.PerformanceId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> GenerateVehicleUtilizationReport(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                ModelState.AddModelError("", "Start date cannot be after end date.");
                return View("Generate");
            }

            var report = await _performanceService.GenerateVehicleUtilizationReportAsync(startDate, endDate);
            report.GeneratedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            TempData["ReportGeneratedMessage"] = $"Vehicle Utilization Report generated successfully (ID: {report.PerformanceId}).";
            return RedirectToAction(nameof(Details), new { id = report.PerformanceId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> GenerateMaintenanceCostReport(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                ModelState.AddModelError("", "Start date cannot be after end date.");
                return View("Generate");
            }

            var report = await _performanceService.GenerateMaintenanceCostReportAsync(startDate, endDate);
            report.GeneratedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            TempData["ReportGeneratedMessage"] = $"Maintenance Cost Report generated successfully (ID: {report.PerformanceId}).";
            return RedirectToAction(nameof(Details), new { id = report.PerformanceId });
        }

        // GET: PerformanceReports/Delete/5
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
