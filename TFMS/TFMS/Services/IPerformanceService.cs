// Services/IPerformanceService.cs

using System.Collections.Generic;
using System.Threading.Tasks;
using TFMS.Models;

namespace TFMS.Services
{
    public interface IPerformanceService
    {
        Task<IEnumerable<PerformanceReport>> GetAllPerformanceReportsAsync();
        Task<PerformanceReport?> GetPerformanceReportByIdAsync(int id);
        Task AddPerformanceReportAsync(PerformanceReport report);
        Task DeletePerformanceReportAsync(int id);
        Task<bool> PerformanceReportExistsAsync(int id);

        // Specific methods for generating reports (these will aggregate data from other entities)
        Task<PerformanceReport> GenerateFuelEfficiencyReportAsync(DateTime startDate, DateTime endDate);
        Task<PerformanceReport> GenerateVehicleUtilizationReportAsync(DateTime startDate, DateTime endDate);
        Task<PerformanceReport> GenerateMaintenanceCostReportAsync(DateTime startDate, DateTime endDate);
    }
}