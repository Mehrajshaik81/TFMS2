// Services/MaintenanceService.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TFMS.Data;
using TFMS.Models;
using System; // For Enum

namespace TFMS.Services
{
    // MaintenanceCostDto is defined in IMaintenanceService.cs now.
    // If you prefer it here, uncomment the class definition.
    // public class MaintenanceCostDto
    // {
    //     public string? MaintenanceType { get; set; }
    //     public decimal TotalCost { get; set; }
    // }

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

            if (!string.IsNullOrEmpty(searchString))
            {
                maintenanceRecords = maintenanceRecords.Where(m => m.Description.Contains(searchString) ||
                                                                 (m.PerformedBy != null && m.PerformedBy.Contains(searchString)));
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                try
                {
                    var parsedStatus = statusFilter.ToEnumValue<MaintenanceStatus>();
                    maintenanceRecords = maintenanceRecords.Where(m => m.Status == parsedStatus);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine($"Warning: Invalid status filter '{statusFilter}' provided for Maintenance Records.");
                }
            }

            if (vehicleIdFilter.HasValue && vehicleIdFilter.Value > 0)
            {
                maintenanceRecords = maintenanceRecords.Where(m => m.VehicleId == vehicleIdFilter.Value);
            }

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

        // New methods for Dashboard implementation
        public async Task<int> GetPendingMaintenanceCountAsync()
        {
            return await _context.MaintenanceRecords.CountAsync(m => m.Status == MaintenanceStatus.Scheduled || m.Status == MaintenanceStatus.InProgress || m.Status == MaintenanceStatus.Delayed);
        }

        public async Task<int> GetOverdueMaintenanceCountAsync()
        {
            return await _context.MaintenanceRecords.CountAsync(m => m.Status == MaintenanceStatus.Overdue);
        }

        public async Task<List<MaintenanceCostDto>> GetMaintenanceCostByTypeAsync()
        {
            return await _context.MaintenanceRecords
                .Where(m => m.Cost.HasValue && m.MaintenanceType != null)
                .GroupBy(m => m.MaintenanceType!) // Use null-forgiving operator after null check
                .Select(g => new MaintenanceCostDto
                {
                    MaintenanceType = g.Key,
                    TotalCost = g.Sum(m => m.Cost.GetValueOrDefault())
                })
                .OrderByDescending(x => x.TotalCost)
                .ToListAsync();
        }
    }
}
