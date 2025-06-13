// Services/MaintenanceService.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TFMS.Data;
using TFMS.Models;

namespace TFMS.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Maintenance>> GetAllMaintenanceRecordsAsync(string? searchString = null, string? statusFilter = null, int? vehicleIdFilter = null, string? maintenanceTypeFilter = null) // <<< MODIFIED
        {
            var maintenanceRecords = _context.MaintenanceRecords
                                                .Include(m => m.Vehicle) // Include Vehicle for filtering/display
                                                .AsQueryable(); // Start with IQueryable

            // Apply search string filter (by Description or PerformedBy)
            if (!string.IsNullOrEmpty(searchString))
            {
                maintenanceRecords = maintenanceRecords.Where(m => m.Description.Contains(searchString) ||
                                                                 (m.PerformedBy != null && m.PerformedBy.Contains(searchString)));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                maintenanceRecords = maintenanceRecords.Where(m => m.Status == statusFilter);
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

            return await maintenanceRecords.ToListAsync(); // Execute query
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
        }

        public async Task UpdateMaintenanceRecordAsync(Maintenance maintenance)
        {
            _context.Update(maintenance);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMaintenanceRecordAsync(int id)
        {
            var maintenance = await _context.MaintenanceRecords.FindAsync(id);
            if (maintenance != null)
            {
                _context.MaintenanceRecords.Remove(maintenance);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> MaintenanceRecordExistsAsync(int id)
        {
            return await _context.MaintenanceRecords.AnyAsync(e => e.MaintenanceId == id);
        }
    }
}