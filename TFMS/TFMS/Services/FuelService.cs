// Services/FuelService.cs
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TFMS.Data;
using TFMS.Models;

namespace TFMS.Services
{
    // DailyFuelConsumptionDto is defined in IFuelService.cs.
    // If you prefer it here, uncomment the class definition.
    // public class DailyFuelConsumptionDto
    // {
    //     public DateTime Date { get; set; }
    //     public decimal Amount { get; set; }
    // }

    public class FuelService : IFuelService
    {
        private readonly ApplicationDbContext _context;

        public FuelService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Corrected signature: driverIdFilter is string, added startDate/endDate
        public async Task<IEnumerable<FuelRecord>> GetAllFuelRecordsAsync(string? searchString = null, int? vehicleIdFilter = null, string? driverIdFilter = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var fuelRecords = _context.FuelRecords
                                     .Include(f => f.Vehicle)
                                     .Include(f => f.Driver)
                                     .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                fuelRecords = fuelRecords.Where(f => f.Location.Contains(searchString) ||
                                                    (f.Driver != null && f.Driver.Email != null && f.Driver.Email.Contains(searchString)) ||
                                                    (f.Vehicle != null && f.Vehicle.RegistrationNumber.Contains(searchString)));
            }

            if (vehicleIdFilter.HasValue && vehicleIdFilter.Value > 0)
            {
                fuelRecords = fuelRecords.Where(f => f.VehicleId == vehicleIdFilter.Value);
            }

            // Fix: DriverId in FuelRecord is string, so directly compare driverIdFilter string
            if (!string.IsNullOrEmpty(driverIdFilter) && driverIdFilter != "0") // Assuming "0" means all drivers in UI
            {
                fuelRecords = fuelRecords.Where(f => f.DriverId == driverIdFilter);
            }

            // Add date filtering
            if (startDate.HasValue)
            {
                fuelRecords = fuelRecords.Where(f => f.Date.HasValue && f.Date.Value.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                fuelRecords = fuelRecords.Where(f => f.Date.HasValue && f.Date.Value.Date <= endDate.Value.Date); // Changed to <= to include end day
            }

            return await fuelRecords.ToListAsync();
        }

        public async Task<FuelRecord?> GetFuelRecordByIdAsync(int id)
        {
            return await _context.FuelRecords
                                 .Include(f => f.Vehicle)
                                 .Include(f => f.Driver)
                                 .FirstOrDefaultAsync(m => m.FuelId == id);
        }

        public async Task AddFuelRecordAsync(FuelRecord fuelRecord)
        {
            _context.Add(fuelRecord);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateFuelRecordAsync(FuelRecord fuelRecord)
        {
            _context.Update(fuelRecord);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFuelRecordAsync(int id)
        {
            var fuelRecord = await _context.FuelRecords.FindAsync(id);
            if (fuelRecord != null)
            {
                _context.FuelRecords.Remove(fuelRecord);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> FuelRecordExistsAsync(int id)
        {
            return await _context.FuelRecords.AnyAsync(e => e.FuelId == id);
        }

        // New methods for Dashboard implementation
        public async Task<decimal> GetTotalFuelCostLastDaysAsync(int days)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            return await _context.FuelRecords
                .Where(f => f.Date.HasValue && f.Date.Value.Date >= cutoffDate.Date && f.Cost.HasValue)
                .SumAsync(f => f.Cost ?? 0);
        }

        public async Task<List<DailyFuelConsumptionDto>> GetFuelConsumptionLastDaysAsync(int days)
        {
            var cutoffDate = DateTime.Today.AddDays(-days);
            return await _context.FuelRecords
                .Where(f => f.Date.HasValue && f.Date.Value.Date >= cutoffDate.Date && f.FuelQuantity.HasValue)
                .GroupBy(f => f.Date!.Value.Date)
                .Select(g => new DailyFuelConsumptionDto
                {
                    Date = g.Key,
                    Amount = g.Sum(f => f.FuelQuantity ?? 0)
                })
                .OrderBy(g => g.Date)
                .ToListAsync();
        }
    }
}
