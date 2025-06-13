// Services/VehicleService.cs
using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Threading.Tasks;
using TFMS.Data;
using TFMS.Models;
using TFMS.Services;

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
            // Ensure the entity is being tracked before updating
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


        public async Task<bool> IsVehicleInUseAsync(int vehicleId)
        {
            return await _context.Trips.AnyAsync(t => t.VehicleId == vehicleId);
        }

    }
}