// Models/FuelRecord.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TFMS.Models; // Ensure this is your correct namespace

namespace TFMS.Models // Your correct namespace
{
    public class FuelRecord
    {
        [Key]
        public int FuelId { get; set; }

        [Required]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; } // Navigation property

        [Display(Name = "Driver")]
        public string? DriverId { get; set; } // Foreign key to ApplicationUser
        [ForeignKey("DriverId")]
        public ApplicationUser? Driver { get; set; } // Navigation property

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime? Date { get; set; } // <<< ENSURE THIS IS NULLABLE

        [Required]
        [Display(Name = "Fuel Quantity (L)")]
        [Column(TypeName = "decimal(18,2)")] // Specify precision and scale for currency/quantity
        public decimal? FuelQuantity { get; set; } // <<< ENSURE THIS IS NULLABLE

        [Required]
        [Display(Name = "Cost")]
        [Column(TypeName = "decimal(18,2)")] // Specify precision and scale
        public decimal? Cost { get; set; } // <<< ENSURE THIS IS NULLABLE

        [Display(Name = "Odometer Reading (km)")]
        public double? OdometerReadingKm { get; set; } // <<< ENSURE THIS IS NULLABLE

        [StringLength(200)]
        public string? Location { get; set; } // Where fuel was filled
    }
}