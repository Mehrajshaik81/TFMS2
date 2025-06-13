// Services/TripService.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TFMS.Data;
using TFMS.Models;

namespace TFMS.Services
{
    public class TripService : ITripService
    {
        private readonly ApplicationDbContext _context;

        public TripService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Trip>> GetAllTripsAsync(string? searchString = null, string? statusFilter = null, string? driverIdFilter = null, int? vehicleIdFilter = null) // <<< MODIFIED
        {
            var trips = _context.Trips
                                .Include(t => t.Vehicle) // Include Vehicle for filtering/display
                                .Include(t => t.Driver)   // Include Driver for filtering/display
                                .AsQueryable(); // Start with IQueryable for chaining filters

            // Apply search string filter (by StartLocation, EndLocation, or RouteDetails)
            if (!string.IsNullOrEmpty(searchString))
            {
                trips = trips.Where(t => t.StartLocation.Contains(searchString) ||
                                       t.EndLocation.Contains(searchString) ||
                                       (t.RouteDetails != null && t.RouteDetails.Contains(searchString))); // Check for null before Contains
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All") // Assume "All" is a default option
            {
                trips = trips.Where(t => t.Status == statusFilter);
            }

            // Apply driver filter by DriverId
            if (!string.IsNullOrEmpty(driverIdFilter) && driverIdFilter != "All")
            {
                trips = trips.Where(t => t.DriverId == driverIdFilter);
            }

            // Apply vehicle filter by VehicleId
            if (vehicleIdFilter.HasValue && vehicleIdFilter.Value > 0) // Assuming 0 or null means "All"
            {
                trips = trips.Where(t => t.VehicleId == vehicleIdFilter.Value);
            }

            return await trips.ToListAsync(); // Execute query
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
    }
}