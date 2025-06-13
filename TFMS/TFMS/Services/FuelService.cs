// Services/FuelService.cs
using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Threading.Tasks;
using TFMS.Data;
using TFMS.Models;
using TFMS.Services;

namespace TFMS.Services
{
    public class FuelService : IFuelService
    {
        private readonly ApplicationDbContext _context;

        public FuelService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FuelRecord>> GetAllFuelRecordsAsync()
        {
            return await _context.FuelRecords.Include(f => f.Vehicle).Include(f => f.Driver).ToListAsync();
        }

        public async Task<FuelRecord?> GetFuelRecordByIdAsync(int id)
        {
            return await _context.FuelRecords.Include(f => f.Vehicle).Include(f => f.Driver).FirstOrDefaultAsync(m => m.FuelId == id);
        }

        public async Task AddFuelRecordAsync(FuelRecord fuelRecord)
        {
            _context.Add(fuelRecord);
            await _context.SaveChangesAsync();

            // Optional: Update vehicle odometer after adding fuel record
            if (fuelRecord.Vehicle != null && fuelRecord.OdometerReadingKm.HasValue)
            {
                fuelRecord.Vehicle.CurrentOdometerKm = fuelRecord.OdometerReadingKm.Value;
                _context.Vehicles.Update(fuelRecord.Vehicle);
                await _context.SaveChangesAsync();
            }
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