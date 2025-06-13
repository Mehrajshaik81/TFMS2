// Models/Maintenance.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TFMS.Models; // Ensure this is your correct namespace

namespace TFMS.Models // Your correct namespace
{
    public class Maintenance
    {
        [Key]
        public int MaintenanceId { get; set; }

        [Required]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; } // Navigation property

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Scheduled Date")]
        public DateTime? ScheduledDate { get; set; } // <<< ENSURE THIS IS NULLABLE

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Scheduled"; // e.g., Scheduled, In Progress, Completed, Overdue, Cancelled

        [DataType(DataType.Date)]
        [Display(Name = "Actual Completion Date")]
        public DateTime? ActualCompletionDate { get; set; } // <<< ENSURE THIS IS NULLABLE

        [Display(Name = "Cost")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Cost { get; set; } // <<< ENSURE THIS IS NULLABLE

        [Display(Name = "Odometer Reading (km)")]
        public double? OdometerReadingKm { get; set; } // <<< ENSURE THIS IS NULLABLE

        [StringLength(100)]
        [Display(Name = "Performed By")]
        public string? PerformedBy { get; set; } // Mechanic name or company

        [StringLength(100)]
        [Display(Name = "Maintenance Type")]
        public string? MaintenanceType { get; set; } // e.g., Routine, Repair, Inspection
    }
}