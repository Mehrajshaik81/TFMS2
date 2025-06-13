// Services/FuelService.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TFMS.Data;
using TFMS.Models;

namespace TFMS.Services
{
    public class FuelService : IFuelService
    {
        private readonly ApplicationDbContext _context;

        public FuelService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FuelRecord>> GetAllFuelRecordsAsync(string? searchString = null, int? vehicleIdFilter = null, string? driverIdFilter = null, DateTime? startDate = null, DateTime? endDate = null) // <<< MODIFIED
        {
            var fuelRecords = _context.FuelRecords
                                        .Include(f => f.Vehicle) // Include Vehicle for filtering/display
                                        .Include(f => f.Driver)   // Include Driver for filtering/display
                                        .AsQueryable(); // Start with IQueryable

            // Apply search string filter (by Location)
            if (!string.IsNullOrEmpty(searchString))
            {
                fuelRecords = fuelRecords.Where(f => (f.Location != null && f.Location.Contains(searchString)));
            }

            // Apply vehicle filter by VehicleId
            if (vehicleIdFilter.HasValue && vehicleIdFilter.Value > 0) // Assuming 0 or null means "All"
            {
                fuelRecords = fuelRecords.Where(f => f.VehicleId == vehicleIdFilter.Value);
            }

            // Apply driver filter by DriverId
            if (!string.IsNullOrEmpty(driverIdFilter) && driverIdFilter != "All")
            {
                fuelRecords = fuelRecords.Where(f => f.DriverId == driverIdFilter);
            }

            // Apply date range filters
            if (startDate.HasValue)
            {
                fuelRecords = fuelRecords.Where(f => f.Date.HasValue && f.Date.Value.Date >= startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                // To include the end date, compare with the start of the next day
                fuelRecords = fuelRecords.Where(f => f.Date.HasValue && f.Date.Value.Date <= endDate.Value.Date);
            }

            return await fuelRecords.ToListAsync(); // Execute query
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
    }
}