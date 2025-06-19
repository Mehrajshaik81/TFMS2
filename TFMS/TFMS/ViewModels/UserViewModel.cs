// TFMS.ViewModels/UserViewModel.cs
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering; // Required for SelectListItem
using System; // Required for DateTime?

namespace TFMS.ViewModels
{
    public class UserViewModel
    {
        public string? Id { get; set; } // Nullable for existing users (Id not set on new)

        [Required(ErrorMessage = "Email is required.")] // Added validation message
        [EmailAddress(ErrorMessage = "Invalid Email Address.")] // Added validation message
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "First Name is required.")] // Added validation message
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required.")] // Added validation message
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Employee ID")]
        public string? EmployeeId { get; set; } // Nullable for optional employee ID

        [Display(Name = "Active Driver")]
        public bool IsActiveDriver { get; set; }

        [Display(Name = "Driving License Number")]
        public string? DrivingLicenseNumber { get; set; } // Nullable

        [Display(Name = "License Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? LicenseExpiryDate { get; set; } // Nullable

        // Property to hold the names of roles CURRENTLY assigned to the user (for display purposes on Index/Details)
        // This is typically populated by _userManager.GetRolesAsync(user)
        public IList<string>? Roles { get; set; } = new List<string>();

        // Property to hold roles selected from the form (checkboxes) during POST (CreateUser/EditUser)
        // This is what the checkbox inputs in Create/Edit views will bind their 'value' to.
        public List<string>? SelectedRoles { get; set; } = new List<string>();

        // Property to hold ALL available roles for rendering checkboxes in EditUser view,
        // allowing pre-selection based on 'Selected' property of SelectListItem.
        public List<SelectListItem>? AvailableRolesList { get; set; } = new List<SelectListItem>();
    }
}
