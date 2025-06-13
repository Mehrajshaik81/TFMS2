// Services/IFuelService.cs

using System.Collections.Generic;
using System.Threading.Tasks;
using TFMS.Models;

namespace TFMS.Services
{
    public interface IFuelService
    {
        Task<IEnumerable<FuelRecord>> GetAllFuelRecordsAsync();
        Task<FuelRecord?> GetFuelRecordByIdAsync(int id);
        Task AddFuelRecordAsync(FuelRecord fuelRecord);
        Task UpdateFuelRecordAsync(FuelRecord fuelRecord);
        Task DeleteFuelRecordAsync(int id);
        Task<bool> FuelRecordExistsAsync(int id);
    }
}