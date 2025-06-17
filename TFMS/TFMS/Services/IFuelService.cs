// Services/IFuelService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TFMS.Models; // For FuelRecord

namespace TFMS.Services
{
    // DTO for daily fuel consumption
    public class DailyFuelConsumptionDto
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; } // Can be total liters or total cost
    }

    public interface IFuelService
    {
        Task<IEnumerable<FuelRecord>> GetAllFuelRecordsAsync(string? searchString = null, int? vehicleIdFilter = null, string? driverIdFilter = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<FuelRecord?> GetFuelRecordByIdAsync(int id);
        Task AddFuelRecordAsync(FuelRecord fuelRecord);
        Task UpdateFuelRecordAsync(FuelRecord fuelRecord);
        Task DeleteFuelRecordAsync(int id);
        Task<bool> FuelRecordExistsAsync(int id);

        // New methods for Dashboard
        Task<decimal> GetTotalFuelCostLastDaysAsync(int days);
        Task<List<DailyFuelConsumptionDto>> GetFuelConsumptionLastDaysAsync(int days);
    }
}
