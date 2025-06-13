// Services/IFuelService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using TFMS.Models;

namespace TFMS.Services
{
    public interface IFuelService
    {
        // Add new parameters for search string, vehicle, driver, and date range
        Task<IEnumerable<FuelRecord>> GetAllFuelRecordsAsync(string? searchString = null, int? vehicleIdFilter = null, string? driverIdFilter = null, DateTime? startDate = null, DateTime? endDate = null); // <<< MODIFIED
        Task<FuelRecord?> GetFuelRecordByIdAsync(int id);
        Task AddFuelRecordAsync(FuelRecord fuelRecord);
        Task UpdateFuelRecordAsync(FuelRecord fuelRecord);
        Task DeleteFuelRecordAsync(int id);
        Task<bool> FuelRecordExistsAsync(int id);
    }
}