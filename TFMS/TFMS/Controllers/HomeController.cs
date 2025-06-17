// Controllers/HomeController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TFMS.Models; // Keep this for your models (Vehicle, Trip, Maintenance etc.)
using TFMS.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using TFMS.Data; // <<< CRITICAL: ONLY THIS using TFMS.Data for EnumExtensions
using System;

namespace TFMS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IVehicleService _vehicleService;
        private readonly ITripService _tripService;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IFuelService _fuelService;
        private readonly IPerformanceService _performanceService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            IVehicleService vehicleService,
            ITripService tripService,
            IMaintenanceService maintenanceService,
            IFuelService fuelService,
            IPerformanceService performanceService,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _vehicleService = vehicleService;
            _tripService = tripService;
            _maintenanceService = maintenanceService;
            _fuelService = fuelService;
            _performanceService = performanceService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Loading Dashboard Index page.");

            // --- Populate General/Admin/Operator Data ---
            ViewBag.TotalVehicles = await _vehicleService.GetTotalVehiclesAsync();
            ViewBag.AvailableVehicles = await _vehicleService.GetAvailableVehiclesCountAsync();
            ViewBag.VehiclesInMaintenance = await _vehicleService.GetVehiclesInMaintenanceCountAsync();
            ViewBag.UnavailableVehicles = await _vehicleService.GetUnavailableVehiclesCountAsync();

            ViewBag.TotalTrips = await _tripService.GetTotalTripsAsync();
            ViewBag.UpcomingTrips = await _tripService.GetUpcomingTripsCountAsync();
            ViewBag.TripsInProgress = await _tripService.GetTripsInProgressCountAsync();
            ViewBag.CompletedTrips = await _tripService.GetCompletedTripsCountAsync();

            ViewBag.PendingMaintenance = await _maintenanceService.GetPendingMaintenanceCountAsync();
            ViewBag.OverdueMaintenance = await _maintenanceService.GetOverdueMaintenanceCountAsync();

            ViewBag.TotalFuelCostLast30Days = await _fuelService.GetTotalFuelCostLastDaysAsync(30);

            // --- Data for Charts ---

            // 1. Vehicle Status Distribution (Pie/Donut Chart)
            var vehicleStatuses = await _vehicleService.GetAllVehiclesAsync();
            var statusCounts = vehicleStatuses
                .GroupBy(v => v.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.VehicleStatusLabels = statusCounts.Select(s => s.Status).ToList();
            ViewBag.VehicleStatusData = statusCounts.Select(s => s.Count).ToList();

            // 2. Maintenance Cost Breakdown by Type (Bar Chart)
            var maintenanceCostByType = await _maintenanceService.GetMaintenanceCostByTypeAsync();
            ViewBag.MaintenanceTypeLabels = maintenanceCostByType.Select(m => m.MaintenanceType).ToList();
            ViewBag.MaintenanceCostData = maintenanceCostByType.Select(m => m.TotalCost).ToList();

            // 3. Last 7 Days Fuel Consumption (Line Chart)
            var last7DaysFuelData = await _fuelService.GetFuelConsumptionLastDaysAsync(7);
            ViewBag.FuelConsumptionDates = last7DaysFuelData.Select(f => f.Date.ToShortDateString()).ToList();
            ViewBag.FuelConsumptionAmounts = last7DaysFuelData.Select(f => f.Amount).ToList();

            // 4. Trip Status Breakdown Chart
            var allTripsForChart = await _tripService.GetAllTripsAsync(null, null, null, null); // Pass nulls for filters here
            var tripStatusCounts = allTripsForChart
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.TripStatusLabels = tripStatusCounts.Select(s => s.Status).ToList();
            ViewBag.TripStatusData = tripStatusCounts.Select(s => s.Count).ToList();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            _logger.LogError("An error occurred. RequestId: {RequestId}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
