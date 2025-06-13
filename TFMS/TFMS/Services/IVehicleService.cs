
using System.Collections.Generic;
using System.Threading.Tasks;
using TFMS.Models;
namespace TFMS.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<Vehicle>> GetAllVehiclesAsync(string? searchString = null, string? statusFilter = null, string? fuelTypeFilter = null); // <<< MODIFIED
        Task<Vehicle?> GetVehicleByIdAsync(int id);
        Task AddVehicleAsync(Vehicle vehicle);
        Task UpdateVehicleAsync(Vehicle vehicle);
        Task DeleteVehicleAsync(int id);
        Task<bool> VehicleExistsAsync(int id);
    }
}