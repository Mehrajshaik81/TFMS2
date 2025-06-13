// Models/Trip.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TFMS.Models; // Ensure this is your correct namespace

namespace TFMS.Models // Your correct namespace
{
    public class Trip
    {
        [Key]
        public int TripId { get; set; }

        [Required]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; } // Navigation property

        [Display(Name = "Driver")]
        public string? DriverId { get; set; } // Foreign key to ApplicationUser (string because IdentityUser.Id is string)
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
        public DateTime? ScheduledStartTime { get; set; } // <<< ENSURE THIS IS NULLABLE

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Scheduled End Time")]
        public DateTime? ScheduledEndTime { get; set; } // <<< ENSURE THIS IS NULLABLE

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // e.g., Pending, In Progress, Completed, Delayed, Canceled

        [DataType(DataType.DateTime)]
        [Display(Name = "Actual Start Time")]
        public DateTime? ActualStartTime { get; set; } // <<< ENSURE THIS IS NULLABLE

        [DataType(DataType.DateTime)]
        [Display(Name = "Actual End Time")]
        public DateTime? ActualEndTime { get; set; } // <<< ENSURE THIS IS NULLABLE

        [Display(Name = "Estimated Distance (km)")]
        public double? EstimatedDistanceKm { get; set; } // <<< ENSURE THIS IS NULLABLE

        [Display(Name = "Actual Distance (km)")]
        public double? ActualDistanceKm { get; set; } // <<< ENSURE THIS IS NULLABLE

        [StringLength(500)]
        [Display(Name = "Route Details")]
        public string? RouteDetails { get; set; }
    }
}