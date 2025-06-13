// Services/IMaintenanceService.cs

using System.Collections.Generic;
using System.Threading.Tasks;
using TFMS.Models;

namespace TFMS.Services
{
    public interface IMaintenanceService
    {
        Task<IEnumerable<Maintenance>> GetAllMaintenanceRecordsAsync();
        Task<Maintenance?> GetMaintenanceByIdAsync(int id);
        Task AddMaintenanceAsync(Maintenance maintenance);
        Task UpdateMaintenanceAsync(Maintenance maintenance);
        Task DeleteMaintenanceAsync(int id);
        Task<bool> MaintenanceExistsAsync(int id);
    }
}