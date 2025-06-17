// Services/ITripService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TFMS.Models; // Ensure correct namespace

namespace TFMS.Services
{
    public interface ITripService
    {
        // Corrected signature: vehicleIdFilter is int?, driverIdFilter is string?
        Task<IEnumerable<Trip>> GetAllTripsAsync(string? searchString = null, string? statusFilter = null, int? vehicleIdFilter = null, string? driverIdFilter = null);
        Task<Trip?> GetTripByIdAsync(int id);
        Task AddTripAsync(Trip trip);
        Task UpdateTripAsync(Trip trip);
        Task DeleteTripAsync(int id);
        Task<bool> TripExistsAsync(int id);

        // New methods for Dashboard (already added in previous steps)
        Task<int> GetTotalTripsAsync();
        Task<int> GetUpcomingTripsCountAsync();
        Task<int> GetTripsInProgressCountAsync();
        Task<int> GetCompletedTripsCountAsync();
    }
}
