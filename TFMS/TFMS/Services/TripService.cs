// Services/TripService.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TFMS.Data;
using TFMS.Models;
using System; // For DateTime

namespace TFMS.Services
{
    public class TripService : ITripService
    {
        private readonly ApplicationDbContext _context;

        public TripService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Corrected signature: DriverId is string, VehicleId is int?
        public async Task<IEnumerable<Trip>> GetAllTripsAsync(string? searchString = null, string? statusFilter = null, int? vehicleIdFilter = null, string? driverIdFilter = null)
        {
            var trips = _context.Trips
                                .Include(t => t.Vehicle)
                                .Include(t => t.Driver)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                trips = trips.Where(t => t.StartLocation.Contains(searchString) ||
                                        t.EndLocation.Contains(searchString) ||
                                        (t.Driver != null && t.Driver.Email != null && t.Driver.Email.Contains(searchString)) ||
                                        (t.Vehicle != null && t.Vehicle.RegistrationNumber.Contains(searchString)));
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                trips = trips.Where(t => t.Status == statusFilter);
            }

            if (vehicleIdFilter.HasValue && vehicleIdFilter.Value > 0)
            {
                trips = trips.Where(t => t.VehicleId == vehicleIdFilter.Value);
            }

            // Fix: DriverId in Trip model is string, so directly compare driverIdFilter string
            if (!string.IsNullOrEmpty(driverIdFilter) && driverIdFilter != "0") // Assuming "0" means all drivers in UI
            {
                // Directly compare the string IDs
                trips = trips.Where(t => t.DriverId == driverIdFilter);
            }

            return await trips.ToListAsync();
        }

        public async Task<Trip?> GetTripByIdAsync(int id)
        {
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

        // New methods for Dashboard implementation
        public async Task<int> GetTotalTripsAsync()
        {
            return await _context.Trips.CountAsync();
        }

        public async Task<int> GetUpcomingTripsCountAsync()
        {
            var today = DateTime.Today;
            return await _context.Trips.CountAsync(t => t.ScheduledStartTime.HasValue && t.ScheduledStartTime.Value.Date >= today && t.Status != "Completed" && t.Status != "Canceled");
        }

        public async Task<int> GetTripsInProgressCountAsync()
        {
            return await _context.Trips.CountAsync(t => t.Status == "In Progress");
        }

        public async Task<int> GetCompletedTripsCountAsync()
        {
            return await _context.Trips.CountAsync(t => t.Status == "Completed");
        }
    }
}
