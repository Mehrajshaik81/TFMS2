using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TFMS.Models;
using TFMS.Services;

namespace TFMS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IVehicleService _vehicleService;
        private readonly ITripService _tripService;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IFuelService _fuelService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            IVehicleService vehicleService,
            ITripService tripService,
            IMaintenanceService maintenanceService,
            IFuelService fuelService,
            UserManager<ApplicationUser> userManager)
        {
            _vehicleService = vehicleService;
            _tripService = tripService;
            _maintenanceService = maintenanceService;
            _fuelService = fuelService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var dashboardViewModel = new DashboardViewModel();
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var allVehicles = await _vehicleService.GetAllVehiclesAsync();
            dashboardViewModel.TotalVehicles = allVehicles.Count();
            dashboardViewModel.InMaintenanceVehicles = allVehicles.Count(v => v.Status == "In Maintenance");

            var allTrips = await _tripService.GetAllTripsAsync();
            dashboardViewModel.TotalTrips = allTrips.Count();

            var today = DateTime.Today;
            var allMaintenance = await _maintenanceService.GetAllMaintenanceRecordsAsync();

            dashboardViewModel.OverdueMaintenanceCount = allMaintenance.Count(m => m.Status == MaintenanceStatus.Overdue);

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var recentFuelRecords = await _fuelService.GetAllFuelRecordsAsync();
            dashboardViewModel.TotalFuelCostLast30Days = recentFuelRecords
                .Where(f => f.Date.HasValue && f.Date.Value.Date >= thirtyDaysAgo.Date && f.Cost.HasValue)
                .Sum(f => f.Cost ?? 0);

            dashboardViewModel.UpcomingTrips = allTrips
                .Where(t => t.ScheduledStartTime.HasValue &&
                            t.ScheduledStartTime.Value.Date >= today &&
                            t.ScheduledStartTime.Value.Date <= today.AddDays(7) &&
                            t.Status != "Completed" &&
                            t.Status != "Canceled")
                .OrderBy(t => t.ScheduledStartTime)
                .Take(5)
                .ToList();

            dashboardViewModel.UpcomingMaintenance = allMaintenance
                .Where(m => m.ScheduledDate.HasValue &&
                            m.ScheduledDate.Value.Date >= today &&
                            m.ScheduledDate.Value.Date <= today.AddDays(30) &&
                            m.Status != MaintenanceStatus.Completed &&
                            m.Status != MaintenanceStatus.Cancelled)
                .OrderBy(m => m.ScheduledDate)
                .Take(5)
                .ToList();

            if (User.IsInRole("Driver") && currentUserId != null)
            {
                dashboardViewModel.DriverAssignedTripsToday = allTrips
                    .Where(t => t.DriverId == currentUserId &&
                                t.ScheduledStartTime.HasValue &&
                                t.ScheduledStartTime.Value.Date == today)
                    .OrderBy(t => t.ScheduledStartTime)
                    .ToList();

                dashboardViewModel.DriverLastFuelRecord = recentFuelRecords
                    .Where(f => f.DriverId == currentUserId)
                    .OrderByDescending(f => f.Date)
                    .FirstOrDefault();

                var driverVehicleIds = allTrips
                    .Where(t => t.DriverId == currentUserId)
                    .Select(t => t.VehicleId)
                    .Distinct()
                    .ToList();

                if (driverVehicleIds.Any())
                {
                    dashboardViewModel.DriverNextMaintenanceForAssignedVehicle = allMaintenance
                        .Where(m => driverVehicleIds.Contains(m.VehicleId) &&
                                    m.ScheduledDate.HasValue &&
                                    m.ScheduledDate.Value.Date >= today &&
                                    m.Status != MaintenanceStatus.Completed &&
                                    m.Status != MaintenanceStatus.Cancelled)
                        .OrderBy(m => m.ScheduledDate)
                        .FirstOrDefault();
                }
            }

            return View(dashboardViewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
