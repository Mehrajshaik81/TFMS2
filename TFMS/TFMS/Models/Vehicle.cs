// Models/Vehicle.cs
using System;
using System.ComponentModel.DataAnnotations;
using TFMS.Models; // Ensure this is your correct namespace

namespace TFMS.Models // Your correct namespace
{ 
    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Capacity (kg/passengers)")]
        public int Capacity { get; set; } // This should be non-nullable, as capacity is usually a definite number

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // e.g., Active, In Maintenance, Out of Service, Retired

        [DataType(DataType.Date)]
        [Display(Name = "Last Serviced Date")]
        public DateTime? LastServicedDate { get; set; } // <<< ENSURE THIS IS NULLABLE

        [Required]
        [StringLength(100)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Model { get; set; } = string.Empty;

        [Display(Name = "Manufacturing Year")]
        public int ManufacturingYear { get; set; } // This should be non-nullable, as year is a definite number

        [Required]
        [StringLength(50)]
        [Display(Name = "Fuel Type")]
        public string FuelType { get; set; } = "Petrol"; // e.g., Petrol, Diesel, Electric

        [Display(Name = "Current Odometer (km)")]
        public double? CurrentOdometerKm { get; set; } // <<< ENSURE THIS IS NULLABLE

        // Navigation properties for related entities
        public ICollection<Trip>? Trips { get; set; }
        public ICollection<FuelRecord>? FuelRecords { get; set; }
        public ICollection<Maintenance>? MaintenanceRecords { get; set; }
    }
}