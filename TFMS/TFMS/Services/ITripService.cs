// Services/ITripService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using TFMS.Models;

namespace TFMS.Services
{
    public interface ITripService
    {
        // Add new parameters for search string, status, driver ID, and vehicle ID
        Task<IEnumerable<Trip>> GetAllTripsAsync(string? searchString = null, string? statusFilter = null, string? driverIdFilter = null, int? vehicleIdFilter = null); // <<< MODIFIED
        Task<Trip?> GetTripByIdAsync(int id);
        Task AddTripAsync(Trip trip);
        Task UpdateTripAsync(Trip trip);
        Task DeleteTripAsync(int id);
        Task<bool> TripExistsAsync(int id);
    }
}