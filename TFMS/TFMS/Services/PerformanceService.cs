// Services/PerformanceService.cs
using Microsoft.EntityFrameworkCore;
 // Your Models namespace
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json; // For serializing report data to JSON
using System.Linq;
using TFMS.Data;
using TFMS.Models;
using TFMS.Services;

namespace TFMS.Services
{
    public class PerformanceService : IPerformanceService
    {
        private readonly ApplicationDbContext _context;

        public PerformanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PerformanceReport>> GetAllPerformanceReportsAsync()
        {
            return await _context.PerformanceReports
                                 .Include(p => p.GeneratedByUser)
                                 .ToListAsync();
        }

        public async Task<PerformanceReport?> GetPerformanceReportByIdAsync(int id)
        {
            return await _context.PerformanceReports
                                 .Include(p => p.GeneratedByUser)
                                 .FirstOrDefaultAsync(m => m.PerformanceId == id);
        }

        public async Task AddPerformanceReportAsync(PerformanceReport report)
        {
            _context.Add(report);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePerformanceReportAsync(int id)
        {
            var report = await _context.PerformanceReports.FindAsync(id);
            if (report != null)
            {
                _context.PerformanceReports.Remove(report);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> PerformanceReportExistsAsync(int id)
        {
            return await _context.PerformanceReports.AnyAsync(e => e.PerformanceId == id);
        }

        // MODIFIED: Added vehicleId parameter and filtering
        public async Task<PerformanceReport> GenerateFuelEfficiencyReportAsync(DateTime startDate, DateTime endDate, int? vehicleId = null)
        {
            var fuelDataQuery = _context.FuelRecords
                .Include(f => f.Vehicle)
                .Where(f => f.Date.HasValue && f.Date.Value.Date >= startDate.Date && f.Date.Value.Date <= endDate.Date && f.OdometerReadingKm.HasValue)
                .AsQueryable();

            if (vehicleId.HasValue && vehicleId.Value > 0)
            {
                fuelDataQuery = fuelDataQuery.Where(f => f.VehicleId == vehicleId.Value);
            }

            // Execute the query to bring data into memory first
            var fuelData = await fuelDataQuery.ToListAsync();

            // Now perform GroupBy and Select using LINQ to Objects (supports ?. )
            var fuelEfficiencyResult = fuelData
                .GroupBy(f => f.Vehicle?.RegistrationNumber ?? "Unknown Vehicle")
                .Select(g => new // Simplified new()
                {
                    Vehicle = g.Key,
                    TotalFuelQuantity = g.Sum(f => f.FuelQuantity ?? 0),
                    TotalCost = g.Sum(f => f.Cost ?? 0),
                    AverageCostPerLiter = g.Sum(f => f.FuelQuantity ?? 0) > 0 ? g.Sum(f => f.Cost ?? 0) / (decimal)g.Sum(f => f.FuelQuantity ?? 0) : 0
                })
                .OrderBy(x => x.Vehicle)
                .ToList(); // <<< CHANGED ToListAsync() to ToList()

            string parametersUsed = $"From: {startDate:yyyy-MM-dd} To: {endDate:yyyy-MM-dd}";
            if (vehicleId.HasValue && vehicleId.Value > 0)
            {
                var selectedVehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicleId.Value);
                if (selectedVehicle != null)
                {
                    parametersUsed += $" | For Vehicle: {selectedVehicle.RegistrationNumber}";
                }
            }

            var report = new PerformanceReport
            {
                ReportType = "Fuel Efficiency Report",
                GeneratedOn = DateTime.UtcNow,
                ParametersUsed = parametersUsed,
                Data = JsonConvert.SerializeObject(fuelEfficiencyResult, Formatting.Indented)
            };

            return report;
        }

        // MODIFIED: Added vehicleId parameter and filtering
        public async Task<PerformanceReport> GenerateVehicleUtilizationReportAsync(DateTime startDate, DateTime endDate, int? vehicleId = null)
        {
            var tripsQuery = _context.Trips
                .Include(t => t.Vehicle)
                .Where(t => t.ScheduledStartTime.HasValue && t.ScheduledStartTime.Value.Date >= startDate.Date && t.ScheduledStartTime.Value.Date <= endDate.Date && t.ActualEndTime.HasValue)
                .AsQueryable();

            if (vehicleId.HasValue && vehicleId.Value > 0)
            {
                tripsQuery = tripsQuery.Where(t => t.VehicleId == vehicleId.Value);
            }

            // Execute the query to bring data into memory first
            var trips = await tripsQuery.ToListAsync();

            // Now perform GroupBy and Select using LINQ to Objects (supports ?. )
            var utilizationResult = trips
                .GroupBy(t => t.Vehicle?.RegistrationNumber ?? "Unknown Vehicle")
                .Select(g => new // Simplified new()
                {
                    Vehicle = g.Key,
                    TotalTrips = g.Count(),
                    TotalActualDistanceKm = g.Sum(t => t.ActualDistanceKm ?? 0),
                    TotalTripDurationHours = g.Sum(t => (t.ActualEndTime - t.ActualStartTime)?.TotalHours ?? 0)
                })
                .OrderBy(x => x.Vehicle)
                .ToList(); // <<< CHANGED ToListAsync() to ToList()

            string parametersUsed = $"From: {startDate:yyyy-MM-dd} To: {endDate:yyyy-MM-dd}";
            if (vehicleId.HasValue && vehicleId.Value > 0)
            {
                var selectedVehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicleId.Value);
                if (selectedVehicle != null)
                {
                    parametersUsed += $" | For Vehicle: {selectedVehicle.RegistrationNumber}";
                }
            }

            var report = new PerformanceReport
            {
                ReportType = "Vehicle Utilization Report",
                GeneratedOn = DateTime.UtcNow,
                ParametersUsed = parametersUsed,
                Data = JsonConvert.SerializeObject(utilizationResult, Formatting.Indented)
            };

            return report;
        }

        // MODIFIED: Added vehicleId parameter and filtering
        public async Task<PerformanceReport> GenerateMaintenanceCostReportAsync(DateTime startDate, DateTime endDate, int? vehicleId = null)
        {
            var maintenanceDataQuery = _context.MaintenanceRecords
                .Include(m => m.Vehicle)
                .Where(m => m.ActualCompletionDate.HasValue && m.ActualCompletionDate.Value.Date >= startDate.Date && m.ActualCompletionDate.Value.Date <= endDate.Date && m.Cost.HasValue)
                .AsQueryable();

            if (vehicleId.HasValue && vehicleId.Value > 0)
            {
                maintenanceDataQuery = maintenanceDataQuery.Where(m => m.VehicleId == vehicleId.Value);
            }

            // Execute the query to bring data into memory first
            var maintenanceData = await maintenanceDataQuery.ToListAsync();

            // Now perform GroupBy and Select using LINQ to Objects (supports ?. )
            var maintenanceCostResult = maintenanceData
                .GroupBy(m => m.Vehicle?.RegistrationNumber ?? "Unknown Vehicle")
                .Select(g => new // Simplified new()
                {
                    Vehicle = g.Key,
                    TotalMaintenanceCost = g.Sum(m => m.Cost ?? 0),
                    NumberOfMaintenanceEvents = g.Count()
                })
                .OrderBy(x => x.Vehicle)
                .ToList(); // <<< CHANGED ToListAsync() to ToList()

            string parametersUsed = $"From: {startDate:yyyy-MM-dd} To: {endDate:yyyy-MM-dd}";
            if (vehicleId.HasValue && vehicleId.Value > 0)
            {
                var selectedVehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicleId.Value);
                if (selectedVehicle != null)
                {
                    parametersUsed += $" | For Vehicle: {selectedVehicle.RegistrationNumber}";
                }
            }

            var report = new PerformanceReport
            {
                ReportType = "Maintenance Cost Report",
                GeneratedOn = DateTime.UtcNow,
                ParametersUsed = parametersUsed,
                Data = JsonConvert.SerializeObject(maintenanceCostResult, Formatting.Indented)
            };

            return report;
        }
    }
}
