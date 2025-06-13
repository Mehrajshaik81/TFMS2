// Services/TripService.cs
using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Threading.Tasks;
using TFMS.Data;
using TFMS.Models;
using TFMS.Services;

namespace TFMS.Services
{
    public class TripService : ITripService
    {
        private readonly ApplicationDbContext _context;

        public TripService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Trip>> GetAllTripsAsync()
        {
            // Include Vehicle and Driver for display purposes
            return await _context.Trips
                                 .Include(t => t.Vehicle)
                                 .Include(t => t.Driver)
                                 .ToListAsync();
        }

        public async Task<Trip?> GetTripByIdAsync(int id)
        {
            // Include Vehicle and Driver for display purposes
            return await _context.Trips
                                 .Include(t => t.Vehicle)
                                 .Include(t => t.Driver)
                                 .FirstOrDefaultAsync(m => m.TripId == id);
        }

        public async Task AddTripAsync(Trip trip)
        {
            _context.Add(trip);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTripAsync(Trip trip)
        {
            _context.Update(trip);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTripAsync(int id)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip != null)
            {
                _context.Trips.Remove(trip);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> TripExistsAsync(int id)
        {
            return await _context.Trips.AnyAsync(e => e.TripId == id);
        }
    }
}