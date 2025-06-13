// Services/MaintenanceService.cs
using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Threading.Tasks;
using TFMS.Data;
using TFMS.Models;
using TFMS.Services;

namespace TFMS.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Maintenance>> GetAllMaintenanceRecordsAsync()
        {
            return await _context.MaintenanceRecords.Include(m => m.Vehicle).ToListAsync();
        }

        public async Task<Maintenance?> GetMaintenanceByIdAsync(int id)
        {
            return await _context.MaintenanceRecords.Include(m => m.Vehicle).FirstOrDefaultAsync(m => m.MaintenanceId == id);
        }

        public async Task AddMaintenanceAsync(Maintenance maintenance)
        {
            _context.Add(maintenance);
            await _context.SaveChangesAsync();

            // Optional: Update vehicle status if it goes into maintenance
            if (maintenance.Vehicle != null && maintenance.Status == "In Progress")
            {
                maintenance.Vehicle.Status = "In Maintenance";
                _context.Vehicles.Update(maintenance.Vehicle);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateMaintenanceAsync(Maintenance maintenance)
        {
            _context.Update(maintenance);
            await _context.SaveChangesAsync();

            // Optional: Update vehicle status if maintenance is completed
            if (maintenance.Vehicle != null && maintenance.Status == "Completed")
            {
                // Check if there are other ongoing maintenance records for this vehicle
                var hasOngoingMaintenance = await _context.MaintenanceRecords
                    .AnyAsync(m => m.VehicleId == maintenance.VehicleId && m.Status == "In Progress" && m.MaintenanceId != maintenance.MaintenanceId);

                if (!hasOngoingMaintenance)
                {
                    maintenance.Vehicle.Status = "Active"; // Set back to active if no other maintenance is ongoing
                    _context.Vehicles.Update(maintenance.Vehicle);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteMaintenanceAsync(int id)
        {
            var maintenance = await _context.MaintenanceRecords.FindAsync(id);
            if (maintenance != null)
            {
                _context.MaintenanceRecords.Remove(maintenance);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> MaintenanceExistsAsync(int id)
        {
            return await _context.MaintenanceRecords.AnyAsync(e => e.MaintenanceId == id);
        }
    }
}