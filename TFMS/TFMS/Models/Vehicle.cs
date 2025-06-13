// Models/Vehicle.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // For [Column]

namespace TFMS.Models
{


    public class Vehicle
    {
        [Key] // Primary Key
        public int VehicleId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        [Display(Name = "Capacity (Tons/CBM)")] // Could be tons or cubic meters
        public double Capacity { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Status")] // e.g., Active, In Maintenance, Out of Service
        public string Status { get; set; } = "Active";

        [DataType(DataType.Date)]
        [Display(Name = "Last Serviced Date")]
        public DateTime? LastServicedDate { get; set; }

        [StringLength(100)]
        public string Make { get; set; } = string.Empty;

        [StringLength(100)]
        public string Model { get; set; } = string.Empty;

        [Range(1900, 2100)]
        public int ManufacturingYear { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Fuel Type")] // e.g., Diesel, Petrol, Electric
        public string FuelType { get; set; } = string.Empty;

        [Display(Name = "Current Odometer (Km)")]
        [Range(0, double.MaxValue)]
        public double CurrentOdometerKm { get; set; }

        // Navigation properties
        public ICollection<Trip>? Trips { get; set; }
        public ICollection<FuelRecord>? FuelRecords { get; set; }
        public ICollection<Maintenance>? MaintenanceRecords { get; set; }
    }
}