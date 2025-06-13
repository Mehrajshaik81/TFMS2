// Services/PerformanceService.cs
using Microsoft.EntityFrameworkCore;
using TFMS.Data;
using TFMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json; // For serializing report data to JSON
using System.Linq; // For LINQ queries

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
            // --- CRITICAL ADDITION: Include GeneratedByUser ---
            return await _context.PerformanceReports.Include(p => p.GeneratedByUser).ToListAsync();
        }

        public async Task<PerformanceReport?> GetPerformanceReportByIdAsync(int id)
        {
            // --- CRITICAL ADDITION: Include GeneratedByUser ---
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

        // --- Report Generation Logic ---
        // (No changes needed in these methods for GeneratedByUser, as it's set in the controller
        // before AddPerformanceReportAsync is called, which then saves it.)

        public async Task<PerformanceReport> GenerateFuelEfficiencyReportAsync(DateTime startDate, DateTime endDate)
        {
            var fuelData = await _context.FuelRecords
                .Include(f => f.Vehicle)
                .Where(f => f.Date >= startDate && f.Date <= endDate && f.OdometerReadingKm.HasValue)
                .ToListAsync();

            var fuelEfficiencyResult = fuelData
                .GroupBy(f => f.Vehicle?.RegistrationNumber ?? "Unknown Vehicle")
                .Select(g => new
                {
                    Vehicle = g.Key,
                    TotalFuelQuantity = g.Sum(f => f.FuelQuantity),
                    TotalCost = g.Sum(f => f.Cost),
                    AverageCostPerLiter = g.Sum(f => f.FuelQuantity) > 0 ? g.Sum(f => f.Cost) / (decimal)g.Sum(f => f.FuelQuantity) : 0
                })
                .OrderBy(x => x.Vehicle)
                .ToList();

            var report = new PerformanceReport
            {
                ReportType = "Fuel Efficiency Report",
                GeneratedOn = DateTime.UtcNow,
                ParametersUsed = $"From: {startDate:yyyy-MM-dd} To: {endDate:yyyy-MM-dd}",
                Data = JsonConvert.SerializeObject(fuelEfficiencyResult, Formatting.Indented)
                // GeneratedByUserId will be set by the controller before AddPerformanceReportAsync is called
            };

            await AddPerformanceReportAsync(report); // Add to DB immediately
            return report; // Return the saved report object
        }

        public async Task<PerformanceReport> GenerateVehicleUtilizationReportAsync(DateTime startDate, DateTime endDate)
        {
            var trips = await _context.Trips
                .Include(t => t.Vehicle)
                .Where(t => t.ScheduledStartTime >= startDate && t.ScheduledStartTime <= endDate && t.ActualEndTime.HasValue)
                .ToListAsync();

            var utilizationResult = trips
                .GroupBy(t => t.Vehicle?.RegistrationNumber ?? "Unknown Vehicle")
                .Select(g => new
                {
                    Vehicle = g.Key,
                    TotalTrips = g.Count(),
                    TotalActualDistanceKm = g.Sum(t => t.ActualDistanceKm ?? 0),
                    TotalTripDurationHours = g.Sum(t => (t.ActualEndTime - t.ActualStartTime)?.TotalHours ?? 0)
                })
                .OrderBy(x => x.Vehicle)
                .ToList();

            var report = new PerformanceReport
            {
                ReportType = "Vehicle Utilization Report",
                GeneratedOn = DateTime.UtcNow,
                ParametersUsed = $"From: {startDate:yyyy-MM-dd} To: {endDate:yyyy-MM-dd}",
                Data = JsonConvert.SerializeObject(utilizationResult, Formatting.Indented)
            };

            await AddPerformanceReportAsync(report);
            return report;
        }

        public async Task<PerformanceReport> GenerateMaintenanceCostReportAsync(DateTime startDate, DateTime endDate)
        {
            var maintenanceData = await _context.MaintenanceRecords
                .Include(m => m.Vehicle)
                .Where(m => m.ActualCompletionDate >= startDate && m.ActualCompletionDate <= endDate && m.Cost.HasValue)
                .ToListAsync();

            var maintenanceCostResult = maintenanceData
                .GroupBy(m => m.Vehicle?.RegistrationNumber ?? "Unknown Vehicle")
                .Select(g => new
                {
                    Vehicle = g.Key,
                    TotalMaintenanceCost = g.Sum(m => m.Cost ?? 0),
                    NumberOfMaintenanceEvents = g.Count()
                })
                .OrderBy(x => x.Vehicle)
                .ToList();

            var report = new PerformanceReport
            {
                ReportType = "Maintenance Cost Report",
                GeneratedOn = DateTime.UtcNow,
                ParametersUsed = $"From: {startDate:yyyy-MM-dd} To: {endDate:yyyy-MM-dd}",
                Data = JsonConvert.SerializeObject(maintenanceCostResult, Formatting.Indented)
            };

            await AddPerformanceReportAsync(report);
            return report;
        }
    }
}
