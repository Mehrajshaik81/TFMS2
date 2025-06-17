// Services/VehicleService.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TFMS.Data;
using TFMS.Models;

namespace TFMS.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly ApplicationDbContext _context;

        public VehicleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync(string? searchString = null, string? statusFilter = null, string? fuelTypeFilter = null)
        {
            var vehicles = _context.Vehicles.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                vehicles = vehicles.Where(v => v.RegistrationNumber.Contains(searchString) ||
                                                v.Make.Contains(searchString) ||
                                                v.Model.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                vehicles = vehicles.Where(v => v.Status == statusFilter);
            }

            if (!string.IsNullOrEmpty(fuelTypeFilter) && fuelTypeFilter != "All")
            {
                vehicles = vehicles.Where(v => v.FuelType == fuelTypeFilter);
            }

            return await vehicles.ToListAsync();
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(int id)
        {
            return await _context.Vehicles.FindAsync(id);
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

        public async Task DeleteVehicleAsync(int id)
        {
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

        // New methods for Dashboard implementation
        public async Task<int> GetTotalVehiclesAsync()
        {
            return await _context.Vehicles.CountAsync();
        }

        public async Task<int> GetAvailableVehiclesCountAsync()
        {
            // This assumes "Active" is your status for available vehicles
            return await _context.Vehicles.CountAsync(v => v.Status == "Active");
        }

        public async Task<int> GetVehiclesInMaintenanceCountAsync()
        {
            // Counts vehicles explicitly marked as "In Maintenance"
            return await _context.Vehicles.CountAsync(v => v.Status == "In Maintenance");
        }

        public async Task<int> GetUnavailableVehiclesCountAsync()
        {
            // Counts vehicles that are not "Active" and not "In Maintenance"
            // You might have other statuses like "Out of Service", "Retired" etc.
            // Adjust logic based on your vehicle status definitions
            return await _context.Vehicles.CountAsync(v => v.Status != "Active" && v.Status != "In Maintenance");
        }
    }
}