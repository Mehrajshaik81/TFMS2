// Models/DashboardViewModel.cs
using System.Collections.Generic;

namespace TFMS.Models // <<< CORRECTED NAMESPACE
{
    public class DashboardViewModel
    {
        // --- General/Admin/Operator Data ---
        public int TotalVehicles { get; set; }
        public int ActiveVehicles { get; set; }
        public int InMaintenanceVehicles { get; set; }
        public int TotalTripsScheduledToday { get; set; }
        public int TripsInProgressToday { get; set; }
        public int TripsCompletedToday { get; set; }
        public int OverdueMaintenanceCount { get; set; }
        public decimal TotalFuelCostLast30Days { get; set; }

        // Lists for specific display (e.g., upcoming trips, recent maintenance)
        public IEnumerable<Trip>? UpcomingTrips { get; set; } // Could be for Admin/Operator
        public IEnumerable<Maintenance>? UpcomingMaintenance { get; set; } // For Admin/Operator

        // --- Driver-Specific Data ---
        public IEnumerable<Trip>? DriverAssignedTripsToday { get; set; } // Trips for the logged-in driver
        public FuelRecord? DriverLastFuelRecord { get; set; } // Last fuel record by the driver
        public Maintenance? DriverNextMaintenanceForAssignedVehicle { get; set; } // If driver has a primary vehicle

        // You can add more properties as needed for summary display
    }
}