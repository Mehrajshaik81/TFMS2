// Models/PerformanceReport.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TFMS.Models;
namespace TFMS.Models
{
    public class PerformanceReport
    {
        [Key]
        public int PerformanceId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Report Type")] // e.g., Fuel Efficiency, Trip Utilization, Maintenance Costs
        public string ReportType { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Generated On")]
        public DateTime GeneratedOn { get; set; } = DateTime.UtcNow;

        // This attribute will store the report data, potentially as JSON
        [Column(TypeName = "nvarchar(MAX)")] // Use nvarchar(MAX) for large strings
        public string? Data { get; set; } // Stores report specific data (e.g., JSON representation of metrics)

        [StringLength(200)]
        [Display(Name = "Parameters Used")] // e.g., "Date Range: 2024-01-01 to 2024-12-31"
        public string? ParametersUsed { get; set; }

        // Optional: Link to the user who generated the report
        public string? GeneratedByUserId { get; set; }
        [ForeignKey("GeneratedByUserId")]
        public ApplicationUser? GeneratedByUser { get; set; }
    }
}