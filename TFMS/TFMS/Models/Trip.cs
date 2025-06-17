// Models/Trip.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // ADD THIS USING DIRECTIVE if not present
using TFMS.Data; // For EnumExtensions (assuming it's here)

namespace TFMS.Models
{
    // Define TripStatus Enum (assuming this is already correct from previous steps)
    public enum TripStatus
    {
        [Display(Name = "Pending")]
        Pending,
        [Display(Name = "In Progress")]
        InProgress,
        [Display(Name = "Completed")]
        Completed,
        [Display(Name = "Delayed")]
        Delayed,
        [Display(Name = "Canceled")]
        Canceled
    }

    public class Trip
    {
        [Key]
        public int TripId { get; set; }

        [Required]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; } // Foreign Key

        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; } // Navigation property
        

        [Display(Name = "Driver")]
        public string DriverId { get; set; } = string.Empty; // Foreign Key to ApplicationUser

        [ForeignKey("DriverId")]
        public ApplicationUser? Driver { get; set; } // Navigation property

        [StringLength(200)]
        [Display(Name = "Start Location")]
        public string StartLocation { get; set; } = string.Empty;


        [StringLength(200)]
        [Display(Name = "End Location")]
        public string EndLocation { get; set; } = string.Empty;

     
        [Display(Name = "Scheduled Start Time")]
        [DataType(DataType.DateTime)]
        public DateTime? ScheduledStartTime { get; set; }

        [Display(Name = "Scheduled End Time")]
        [DataType(DataType.DateTime)]
        public DateTime? ScheduledEndTime { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = TripStatus.Pending.ToString(); // Default status to "Pending"

        [Display(Name = "Actual Start Time")]
        [DataType(DataType.DateTime)]
        public DateTime? ActualStartTime { get; set; }

        [Display(Name = "Actual End Time")]
        [DataType(DataType.DateTime)]
        public DateTime? ActualEndTime { get; set; }

        [Display(Name = "Estimated Distance (km)")]
        [Column(TypeName = "decimal(18, 2)")] // <<< ADD THIS LINE
        public decimal? EstimatedDistanceKm { get; set; }

        [Display(Name = "Actual Distance (km)")]
        [Column(TypeName = "decimal(18, 2)")] // <<< ADD THIS LINE
        public decimal? ActualDistanceKm { get; set; }

        [StringLength(500)]
        [Display(Name = "Route Details")]
        public string? RouteDetails { get; set; }
    }
}
