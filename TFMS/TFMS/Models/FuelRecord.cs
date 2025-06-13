// Models/FuelRecord.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TFMS.Models;
namespace TFMS.Models
{


    public class FuelRecord
    {
        [Key]
        public int FuelId { get; set; }

        [Required]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; } // Foreign Key

        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; } // Navigation property

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Fueling")]
        public DateTime Date { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        [Display(Name = "Fuel Quantity (Liters)")]
        public double FuelQuantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")] // For precise currency handling
        public decimal Cost { get; set; }

        [Display(Name = "Odometer Reading (Km)")]
        [Range(0, double.MaxValue)]
        public double? OdometerReadingKm { get; set; } // Optional, but highly recommended for efficiency calculation

        [StringLength(200)]
        public string? Location { get; set; }

        // Optional: Link to the driver who filled the fuel
        public string? DriverId { get; set; }
        [ForeignKey("DriverId")]
        public ApplicationUser? Driver { get; set; }
    }
}
