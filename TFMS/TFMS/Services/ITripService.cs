// Services/ITripService.cs

using System.Collections.Generic;
using System.Threading.Tasks;
using TFMS.Models;

namespace TFMS.Services
{
    public interface ITripService
    {
        Task<IEnumerable<Trip>> GetAllTripsAsync();
        Task<Trip?> GetTripByIdAsync(int id);
        Task AddTripAsync(Trip trip);
        Task UpdateTripAsync(Trip trip);
        Task DeleteTripAsync(int id);
        Task<bool> TripExistsAsync(int id);
    }
}