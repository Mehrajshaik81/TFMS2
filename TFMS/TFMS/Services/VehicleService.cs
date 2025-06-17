// Services/VehicleService.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TFMS.Data;
using TFMS.Models;
using System; // For DateTime.Today

namespace TFMS.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly ApplicationDbContext _context;

        public VehicleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
        {
            return await _context.Vehicles.ToListAsync();
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(int id)
        {
            return await _context.Vehicles.FirstOrDefaultAsync(m => m.VehicleId == id);
        }

        public async Task AddVehicleAsync(Vehicle vehicle)
        {
            _context.Add(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateVehicleAsync(Vehicle vehicle)
        {
            _context.Update(vehicle);
            await _context.SaveChangesAsync();
        }

        // Modified DeleteVehicleAsync to NOT delete if associated trips exist
        public async Task DeleteVehicleAsync(int id)
        {
            // IMPORTANT: This method now assumes the controller will check HasAssociatedTripsAsync
            // and only call this if no trips are associated.
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> VehicleExistsAsync(int id)
        {
            return await _context.Vehicles.AnyAsync(e => e.VehicleId == id);
        }

        // Dashboard related methods
        public async Task<int> GetTotalVehiclesAsync()
        {
            return await _context.Vehicles.CountAsync();
        }

        public async Task<int> GetAvailableVehiclesCountAsync()
        {
            return await _context.Vehicles.CountAsync(v => v.Status == "Available");
        }

        public async Task<int> GetVehiclesInMaintenanceCountAsync()
        {
            return await _context.Vehicles.CountAsync(v => v.Status == "In Maintenance");
        }

        public async Task<int> GetUnavailableVehiclesCountAsync()
        {
            // Assuming "Unavailable" could be other statuses like "Out of Service", "Retired"
            // For now, let's count vehicles not "Available" or "In Maintenance"
            return await _context.Vehicles.CountAsync(v => v.Status != "Available" && v.Status != "In Maintenance");
        }

        // NEW: Implementation for HasAssociatedTripsAsync
        public async Task<bool> HasAssociatedTripsAsync(int vehicleId)
        {
            // Check if any trips reference this VehicleId
            return await _context.Trips.AnyAsync(t => t.VehicleId == vehicleId);
        }
    }
}
