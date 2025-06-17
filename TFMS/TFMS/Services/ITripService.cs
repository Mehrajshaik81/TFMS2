// Services/ITripService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TFMS.Models; // Ensure correct namespace

namespace TFMS.Services
{
    // DTO for daily fuel consumption (kept for reference from previous steps, belongs to FuelService usually)
    // public class DailyFuelConsumptionDto
    // {
    //     public DateTime Date { get; set; }
    //     public decimal Amount { get; set; }
    // }

    public interface ITripService
    {
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

        // NEW: Method to get trips for a specific driver
        Task<IEnumerable<Trip>> GetTripsByDriverIdAsync(string driverId);
    }
}
