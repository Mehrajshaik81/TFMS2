// Services/IMaintenanceService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using TFMS.Models; // For Maintenance

namespace TFMS.Services
{
    // DTO for Maintenance Cost by Type
    public class MaintenanceCostDto
    {
        public string? MaintenanceType { get; set; }
        public decimal TotalCost { get; set; }
    }

    public interface IMaintenanceService
    {
        Task<IEnumerable<Maintenance>> GetAllMaintenanceRecordsAsync(string? searchString = null, string? statusFilter = null, int? vehicleIdFilter = null, string? maintenanceTypeFilter = null);
        Task<Maintenance?> GetMaintenanceRecordByIdAsync(int id);
        Task AddMaintenanceRecordAsync(Maintenance maintenance);
        Task UpdateMaintenanceRecordAsync(Maintenance maintenance);
        Task DeleteMaintenanceRecordAsync(int id);
        Task<bool> MaintenanceRecordExistsAsync(int id);

        // New methods for Dashboard
        Task<int> GetPendingMaintenanceCountAsync();
        Task<int> GetOverdueMaintenanceCountAsync();
        Task<List<MaintenanceCostDto>> GetMaintenanceCostByTypeAsync();
    }
}
