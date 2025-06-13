// ViewModels/UserViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TFMS.Models; // Ensure this matches your ApplicationUser namespace

namespace TFMS.ViewModels // <<< Your correct namespace for ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty; // User ID from Identity

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Employee ID")]
        public string? EmployeeId { get; set; }

        [Display(Name = "Is Active Driver")]
        public bool IsActiveDriver { get; set; } = false;

        [StringLength(50)]
        [Display(Name = "Driving License Number")]
        public string? DrivingLicenseNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "License Expiry Date")]
        public DateTime? LicenseExpiryDate { get; set; }

        // Properties for Role Management
        [Display(Name = "Assigned Roles")]
        public IList<string> Roles { get; set; } = new List<string>();

        // For password reset (only needed for create/edit)
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}