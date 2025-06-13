// Controllers/HomeController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // Required for UserManager and getting current user ID
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TFMS.Models; // Corrected Namespace
using TFMS.Services; // Corrected Namespace
using TFMS.Data; // Corrected Namespace
using System.Security.Claims; // Required for ClaimTypes.NameIdentifier
using System.Linq; // For LINQ queries

namespace TFMS.Controllers // Corrected Namespace
{
    [Authorize] // Ensure users are logged in to see the dashboard
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IVehicleService _vehicleService;
        private readonly ITripService _tripService;
        private readonly IFuelService _fuelService;
        private readonly IMaintenanceService _maintenanceService;
        private readonly UserManager<ApplicationUser> _userManager; // To get user details and roles

        public HomeController(ILogger<HomeController> logger,
                                  IVehicleService vehicleService,
                                  ITripService tripService,
                                  IFuelService fuelService,
                                  IMaintenanceService maintenanceService,
                                  UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _vehicleService = vehicleService;
            _tripService = tripService;
            _fuelService = fuelService;
            _maintenanceService = maintenanceService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel();
            var currentUser = await _userManager.GetUserAsync(User); // Get the currently logged-in user object
            var userId = currentUser?.Id;

            // --- Common Data for All Roles (or just for Admin/Operator if desired) ---
            var allVehicles = await _vehicleService.GetAllVehiclesAsync();
            viewModel.TotalVehicles = allVehicles.Count();
            viewModel.ActiveVehicles = allVehicles.Count(v => v.Status == "Active");
            viewModel.InMaintenanceVehicles = allVehicles.Count(v => v.Status == "In Maintenance");

            var allTrips = await _tripService.GetAllTripsAsync();
            var today = DateTime.Today;

            // FIX: Removed .HasValue from non-nullable DateTime properties like ScheduledStartTime.Value.Date
            viewModel.TotalTripsScheduledToday = allTrips.Count(t => t.ScheduledStartTime.HasValue && t.ScheduledStartTime.Value.Date == today);
            viewModel.TripsInProgressToday = allTrips.Count(t => t.Status == "In Progress" && t.ScheduledStartTime.HasValue && t.ScheduledStartTime.Value.Date == today);
            viewModel.TripsCompletedToday = allTrips.Count(t => t.Status == "Completed" && t.ActualEndTime.HasValue && t.ActualEndTime.Value.Date == today);

            var allMaintenance = await _maintenanceService.GetAllMaintenanceRecordsAsync();
            viewModel.OverdueMaintenanceCount = allMaintenance.Count(m => m.Status != "Completed" && m.ScheduledDate.HasValue && m.ScheduledDate.Value.Date < today);

            var allFuelRecords = await _fuelService.GetAllFuelRecordsAsync();
            viewModel.TotalFuelCostLast30Days = allFuelRecords
                .Where(f => f.Date.HasValue && f.Date.Value.Date >= today.AddDays(-30))
                .Sum(f => f.Cost ?? 0);


            // --- Role-Specific Data Fetching ---

            if (User.IsInRole("Fleet Administrator") || User.IsInRole("Fleet Operator"))
            {
                // Admin/Operator specific data
                viewModel.UpcomingTrips = allTrips
                    .Where(t => t.Status == "Pending" && t.ScheduledStartTime.HasValue && t.ScheduledStartTime.Value.Date >= today)
                    .OrderBy(t => t.ScheduledStartTime)
                    .Take(5); // Show top 5 upcoming

                viewModel.UpcomingMaintenance = allMaintenance
                    .Where(m => m.Status != "Completed" && m.ScheduledDate.HasValue && m.ScheduledDate.Value.Date >= today)
                    .OrderBy(m => m.ScheduledDate)
                    .Take(5); // Show top 5 upcoming
            }

            if (User.IsInRole("Driver") && userId != null)
            {
                // Driver specific data
                viewModel.DriverAssignedTripsToday = allTrips
                    .Where(t => t.DriverId == userId && t.ScheduledStartTime.HasValue && t.ScheduledStartTime.Value.Date == today)
                    .OrderBy(t => t.ScheduledStartTime)
                    .ToList();

                viewModel.DriverLastFuelRecord = allFuelRecords
                    .Where(f => f.DriverId == userId)
                    .OrderByDescending(f => f.Date)
                    .FirstOrDefault();

                // Logic for DriverNextMaintenanceForAssignedVehicle
                // This assumes a driver might have a primary vehicle, or we find the vehicle they last drove
                // FIX: Removed .HasValue from t.VehicleId because VehicleId is a non-nullable int
                var driverVehicles = allTrips.Where(t => t.DriverId == userId)
                                            .Select(t => t.VehicleId)
                                            .Distinct()
                                            .ToList();

                if (driverVehicles.Any())
                {
                    viewModel.DriverNextMaintenanceForAssignedVehicle = allMaintenance
                        .Where(m => driverVehicles.Contains(m.VehicleId) && m.Status != "Completed" && m.ScheduledDate.HasValue && m.ScheduledDate.Value.Date >= today)
                        .OrderBy(m => m.ScheduledDate)
                        .FirstOrDefault();
                }
            }

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
