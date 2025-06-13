// Services/IMaintenanceService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using TFMS.Models;

namespace TFMS.Services
{
    public interface IMaintenanceService
    {
        // Add new parameters for search string, status, vehicle, and maintenance type
        Task<IEnumerable<Maintenance>> GetAllMaintenanceRecordsAsync(string? searchString = null, string? statusFilter = null, int? vehicleIdFilter = null, string? maintenanceTypeFilter = null); // <<< MODIFIED
        Task<Maintenance?> GetMaintenanceRecordByIdAsync(int id);
        Task AddMaintenanceRecordAsync(Maintenance maintenance);
        Task UpdateMaintenanceRecordAsync(Maintenance maintenance);
        Task DeleteMaintenanceRecordAsync(int id);
        Task<bool> MaintenanceRecordExistsAsync(int id);
    }
}