// Services/MaintenanceService.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TFMS.Data; // Ensure correct namespace, includes EnumExtensions
using TFMS.Models; // Ensure correct namespace
using System; // For Enum

namespace TFMS.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IVehicleService _vehicleService;

        public MaintenanceService(ApplicationDbContext context, IVehicleService vehicleService)
        {
            _context = context;
            _vehicleService = vehicleService;
        }

        public async Task<IEnumerable<Maintenance>> GetAllMaintenanceRecordsAsync(string? searchString = null, string? statusFilter = null, int? vehicleIdFilter = null, string? maintenanceTypeFilter = null)
        {
            var maintenanceRecords = _context.MaintenanceRecords
                                                .Include(m => m.Vehicle)
                                                .AsQueryable();

            // Apply search string filter
            if (!string.IsNullOrEmpty(searchString))
            {
                maintenanceRecords = maintenanceRecords.Where(m => m.Description.Contains(searchString) ||
                                                                 (m.PerformedBy != null && m.PerformedBy.Contains(searchString)));
            }

            // Apply status filter (now relies on the string from UI matching enum description/name)
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                // Use the ToEnumValue extension method for robust parsing from string filter
                try
                {
                    var parsedStatus = statusFilter.ToEnumValue<MaintenanceStatus>(); // Use the new extension method
                    maintenanceRecords = maintenanceRecords.Where(m => m.Status == parsedStatus);
                }
                catch (ArgumentException)
                {
                    // Handle case where statusFilter string doesn't match any enum value/description
                    // e.g., log error or return empty set, or ignore filter
                    Console.WriteLine($"Warning: Invalid status filter '{statusFilter}' provided for Maintenance Records.");
                    // You might choose to return an empty list or keep the unfiltered query
                    // For now, we'll let it proceed without this filter.
                }
            }

            // Apply vehicle filter by VehicleId
            if (vehicleIdFilter.HasValue && vehicleIdFilter.Value > 0)
            {
                maintenanceRecords = maintenanceRecords.Where(m => m.VehicleId == vehicleIdFilter.Value);
            }

            // Apply maintenance type filter
            if (!string.IsNullOrEmpty(maintenanceTypeFilter) && maintenanceTypeFilter != "All")
            {
                maintenanceRecords = maintenanceRecords.Where(m => m.MaintenanceType == maintenanceTypeFilter);
            }

            return await maintenanceRecords.ToListAsync();
        }

        public async Task<Maintenance?> GetMaintenanceRecordByIdAsync(int id)
        {
            return await _context.MaintenanceRecords
                                 .Include(m => m.Vehicle)
                                 .FirstOrDefaultAsync(m => m.MaintenanceId == id);
        }

        public async Task AddMaintenanceRecordAsync(Maintenance maintenance)
        {
            _context.Add(maintenance);
            await _context.SaveChangesAsync();
            await UpdateVehicleStatusAfterMaintenanceChange(maintenance.VehicleId);
        }

        public async Task UpdateMaintenanceRecordAsync(Maintenance maintenance)
        {
            _context.Update(maintenance);
            await _context.SaveChangesAsync();
            await UpdateVehicleStatusAfterMaintenanceChange(maintenance.VehicleId);
        }

        public async Task DeleteMaintenanceRecordAsync(int id)
        {
            var maintenance = await _context.MaintenanceRecords.FindAsync(id);
            if (maintenance != null)
            {
                _context.MaintenanceRecords.Remove(maintenance);
                await _context.SaveChangesAsync();
                await UpdateVehicleStatusAfterMaintenanceChange(maintenance.VehicleId);
            }
        }

        public async Task<bool> MaintenanceRecordExistsAsync(int id)
        {
            return await _context.MaintenanceRecords.AnyAsync(e => e.MaintenanceId == id);
        }

        private async Task UpdateVehicleStatusAfterMaintenanceChange(int vehicleId)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId);
            if (vehicle == null) return;

            // Check for any outstanding maintenance (Scheduled, InProgress, Overdue, Delayed)
            var outstandingMaintenance = await _context.MaintenanceRecords
                .Where(m => m.VehicleId == vehicleId &&
                            (m.Status == MaintenanceStatus.Scheduled ||
                             m.Status == MaintenanceStatus.InProgress ||
                             m.Status == MaintenanceStatus.Overdue ||
                             m.Status == MaintenanceStatus.Delayed))
                .AnyAsync();

            if (outstandingMaintenance)
            {
                vehicle.Status = "In Maintenance";
            }
            else
            {
                vehicle.Status = "Active";
            }

            await _vehicleService.UpdateVehicleAsync(vehicle);
        }
    }
}