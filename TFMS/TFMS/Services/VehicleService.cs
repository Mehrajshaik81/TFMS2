// Services/VehicleService.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq; // Add this using directive if not already present
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

        public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync(string? searchString = null, string? statusFilter = null, string? fuelTypeFilter = null) // <<< MODIFIED
        {
            var vehicles = _context.Vehicles.AsQueryable(); // Start with IQueryable

            // Apply search string filter
            if (!string.IsNullOrEmpty(searchString))
            {
                vehicles = vehicles.Where(v => v.RegistrationNumber.Contains(searchString) ||
                                               v.Make.Contains(searchString) ||
                                               v.Model.Contains(searchString));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All") // Assume "All" is a default option
            {
                vehicles = vehicles.Where(v => v.Status == statusFilter);
            }

            // Apply fuel type filter
            if (!string.IsNullOrEmpty(fuelTypeFilter) && fuelTypeFilter != "All") // Assume "All" is a default option
            {
                vehicles = vehicles.Where(v => v.FuelType == fuelTypeFilter);
            }

            return await vehicles.ToListAsync(); // Execute query
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
    }
}