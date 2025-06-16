// Models/DashboardViewModel.cs
using System.Collections.Generic;

namespace TFMS.Models // Your correct namespace
{
    public class DashboardViewModel
    {
        // --- General/Admin/Operator Data ---
        public int TotalVehicles { get; set; }
        // Removed ActiveVehicles as per request to consolidate trip metrics
        public int InMaintenanceVehicles { get; set; }

        // Consolidated all trip metrics into a single TotalTrips property
        public int TotalTrips { get; set; } // NEW: Total count of all trips in the system

        public int OverdueMaintenanceCount { get; set; }
        public decimal TotalFuelCostLast30Days { get; set; }

        // Lists for specific display (e.g., upcoming trips, recent maintenance)
        public IEnumerable<Trip>? UpcomingTrips { get; set; }
        public IEnumerable<Maintenance>? UpcomingMaintenance { get; set; }

        // --- Driver-Specific Data ---
        public IEnumerable<Trip>? DriverAssignedTripsToday { get; set; }
        public FuelRecord? DriverLastFuelRecord { get; set; }
        public Maintenance? DriverNextMaintenanceForAssignedVehicle { get; set; }

        // You can add more properties as needed for summary display
    }
}
