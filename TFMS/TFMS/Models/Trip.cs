// Models/Trip.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TFMS.Models;
namespace TFMS.Models
{
    public class Trip
    {
        [Key]
        public int TripId { get; set; }

        [Required]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; } // Foreign Key

        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; } // Navigation property

        [Required]
        [Display(Name = "Driver")]
        public string DriverId { get; set; } = string.Empty; // Foreign Key (string because IdentityUser.Id is string)

        [ForeignKey("DriverId")]
        public ApplicationUser? Driver { get; set; } // Navigation property

        [Required]
        [StringLength(200)]
        [Display(Name = "Start Location")]
        public string StartLocation { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "End Location")]
        public string EndLocation { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Scheduled Start Time")]
        public DateTime ScheduledStartTime { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Scheduled End Time")]
        public DateTime ScheduledEndTime { get; set; }

        [StringLength(50)]
        [Display(Name = "Current Status")] // e.g., Pending, In Progress, Completed, Delayed, Canceled
        public string Status { get; set; } = "Pending";

        [DataType(DataType.DateTime)]
        [Display(Name = "Actual Start Time")]
        public DateTime? ActualStartTime { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Actual End Time")]
        public DateTime? ActualEndTime { get; set; }

        [Display(Name = "Estimated Distance (Km)")]
        [Range(0, double.MaxValue)]
        public double EstimatedDistanceKm { get; set; }

        [Display(Name = "Actual Distance (Km)")]
        [Range(0, double.MaxValue)]
        public double? ActualDistanceKm { get; set; }

        [StringLength(500)]
        [Display(Name = "Route Details")]
        public string? RouteDetails { get; set; } // Could be a JSON string for complex route info
    }
}