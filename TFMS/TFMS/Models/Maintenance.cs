// Models/Maintenance.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TFMS.Models;
namespace TFMS.Models
{
    public class Maintenance
    {
        [Key]
        public int MaintenanceId { get; set; }

        [Required]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; } // Foreign Key

        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; } // Navigation property

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Scheduled Date")]
        public DateTime ScheduledDate { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Status")] // e.g., Scheduled, Completed, Overdue, Cancelled
        public string Status { get; set; } = "Scheduled";

        [DataType(DataType.Date)]
        [Display(Name = "Actual Completion Date")]
        public DateTime? ActualCompletionDate { get; set; }

        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")] // For precise currency handling
        public decimal? Cost { get; set; }

        [Display(Name = "Odometer Reading (Km)")]
        [Range(0, double.MaxValue)]
        public double? OdometerReadingKm { get; set; }

        [StringLength(200)]
        [Display(Name = "Performed By")] // e.g., Internal team, Vendor name
        public string? PerformedBy { get; set; }

        [StringLength(50)]
        [Display(Name = "Maintenance Type")] // e.g., Preventive, Corrective, Repair
        public string? MaintenanceType { get; set; }
    }
}