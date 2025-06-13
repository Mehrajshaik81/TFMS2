// Controllers/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList
using Microsoft.EntityFrameworkCore; // For ToListAsync
using TFMS.Models; // Your ApplicationUser model
using TFMS.ViewModels; // Your UserViewModel

namespace TFMS.Controllers // <<< Your correct namespace for Controllers
{
    [Authorize(Roles = "Fleet Administrator")] // Only Fleet Administrators can access this controller
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "N/A",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    EmployeeId = user.EmployeeId,
                    IsActiveDriver = user.IsActiveDriver,
                    DrivingLicenseNumber = user.DrivingLicenseNumber,
                    LicenseExpiryDate = user.LicenseExpiryDate,
                    Roles = await _userManager.GetRolesAsync(user) // Get roles for each user
                });
            }
            return View(userViewModels);
        }

        // GET: Admin/CreateUser
        public IActionResult CreateUser()
        {
            ViewBag.AvailableRoles = new SelectList(_roleManager.Roles.Select(r => r.Name));
            return View();
        }

        // POST: Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(UserViewModel model)
        {
            // Clear password validation if it's empty but model state expects it (e.g. for updates)
            // For creation, it should be required, so we'll check later
            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.Remove("Password");
                ModelState.Remove("ConfirmPassword");
            }


            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmployeeId = model.EmployeeId,
                    IsActiveDriver = model.IsActiveDriver,
                    DrivingLicenseNumber = model.DrivingLicenseNumber,
                    LicenseExpiryDate = model.LicenseExpiryDate,
                    EmailConfirmed = true // Automatically confirm email for admin-created users
                };

                // Password is required for new user creation
                if (string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError("", "Password is required for new user creation.");
                    ViewBag.AvailableRoles = new SelectList(_roleManager.Roles.Select(r => r.Name));
                    return View(model);
                }

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign roles
                    if (model.Roles != null && model.Roles.Any())
                    {
                        await _userManager.AddToRolesAsync(user, model.Roles);
                    }
                    TempData["SuccessMessage"] = $"User '{user.Email}' created successfully.";
                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            ViewBag.AvailableRoles = new SelectList(_roleManager.Roles.Select(r => r.Name));
            return View(model);
        }

        // GET: Admin/EditUser/5
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            var model = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "N/A",
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmployeeId = user.EmployeeId,
                IsActiveDriver = user.IsActiveDriver,
                DrivingLicenseNumber = user.DrivingLicenseNumber,
                LicenseExpiryDate = user.LicenseExpiryDate,
                Roles = userRoles.ToList()
            };

            ViewBag.AvailableRoles = new SelectList(allRoles); // All available roles
            return View(model);
        }

        // POST: Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserViewModel model)
        {
            // Passwords are optional for edits, only if provided
            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.Remove("Password");
                ModelState.Remove("ConfirmPassword");
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                // Update user properties
                user.Email = model.Email;
                user.UserName = model.Email; // Keep UserName and Email in sync for Identity
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.EmployeeId = model.EmployeeId;
                user.IsActiveDriver = model.IsActiveDriver;
                user.DrivingLicenseNumber = model.DrivingLicenseNumber;
                user.LicenseExpiryDate = model.LicenseExpiryDate;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Update roles
                    var existingRoles = await _userManager.GetRolesAsync(user);
                    var rolesToRemove = existingRoles.Except(model.Roles ?? new List<string>());
                    var rolesToAdd = (model.Roles ?? new List<string>()).Except(existingRoles);

                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    await _userManager.AddToRolesAsync(user, rolesToAdd);

                    // Update password if provided
                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var passwordResetResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
                        if (!passwordResetResult.Succeeded)
                        {
                            foreach (var error in passwordResetResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            ViewBag.AvailableRoles = new SelectList(_roleManager.Roles.Select(r => r.Name));
                            return View(model);
                        }
                    }

                    TempData["SuccessMessage"] = $"User '{user.Email}' updated successfully.";
                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            ViewBag.AvailableRoles = new SelectList(_roleManager.Roles.Select(r => r.Name));
            return View(model);
        }


        // GET: Admin/DeleteUser/5
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var model = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "N/A",
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmployeeId = user.EmployeeId,
                IsActiveDriver = user.IsActiveDriver,
                DrivingLicenseNumber = user.DrivingLicenseNumber,
                LicenseExpiryDate = user.LicenseExpiryDate,
                Roles = userRoles.ToList()
            };

            return View(model);
        }

        // POST: Admin/DeleteUser/5
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User '{user.Email}' deleted successfully.";
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View("DeleteUser", new UserViewModel { Id = id, Email = user.Email ?? "N/A" }); // Pass model to re-render view with errors
        }
    }
}