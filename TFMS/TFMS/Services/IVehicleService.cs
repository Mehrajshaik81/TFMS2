// Services/IVehicleService.cs
using TFMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace TFMS.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<Vehicle>> GetAllVehiclesAsync();
        Task<Vehicle?> GetVehicleByIdAsync(int id);
        Task AddVehicleAsync(Vehicle vehicle);
        Task UpdateVehicleAsync(Vehicle vehicle);
        Task DeleteVehicleAsync(int id);
        Task<bool> VehicleExistsAsync(int id); // Helper method
    }
}