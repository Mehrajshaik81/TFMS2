// Controllers/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList, SelectListItem
using Microsoft.EntityFrameworkCore; // For ToListAsync
using TFMS.Models; // Your ApplicationUser model
using TFMS.ViewModels; // Your UserViewModel
using System.Collections.Generic; // For List
using System.Linq; // For LINQ operations
using System.Threading.Tasks; // For async/await
using System.Security.Claims; // For ClaimTypes
using System; // For DateTime

namespace TFMS.Controllers
{
    [Authorize(Roles = "Fleet Administrator")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin/Users (List Users)
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
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
                    Roles = roles.ToList()
                });
            }
            return View(userViewModels);
        }

        // GET: Admin/DetailsUser/{id}
        public async Task<IActionResult> DetailsUser(string id)
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


        // GET: Admin/CreateUser
        public async Task<IActionResult> CreateUser() // Made async to use await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        {
            var model = new UserViewModel();
            // Materialize role names first
            var allRoleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            model.AvailableRolesList = allRoleNames.Select(rName => new SelectListItem { Text = rName, Value = rName }).ToList();
            return View(model);
        }

        // POST: Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(UserViewModel model)
        {
            // Removed ModelState.Remove for password/confirmPassword for Create action
            // as they are [Required] and should be validated by ModelState.IsValid.

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
                    EmailConfirmed = true
                };

                // Password should be guaranteed non-null by [Required] and ModelState.IsValid,
                // added null-forgiving operator for CreateAsync signature.
                var result = await _userManager.CreateAsync(user, model.Password!);

                if (result.Succeeded)
                {
                    if (model.SelectedRoles != null && model.SelectedRoles.Any())
                    {
                        await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    }
                    TempData["SuccessMessage"] = $"User '{user.Email}' created successfully.";
                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            // Repopulate AvailableRolesList if validation fails
            var allRoleNamesForFail = await _roleManager.Roles.Select(r => r.Name).ToListAsync(); // Materialize
            model.AvailableRolesList = allRoleNamesForFail.Select(rName => new SelectListItem
            {
                Text = rName,
                Value = rName,
                Selected = model.SelectedRoles?.Contains(rName) ?? false // Now safe
            }).ToList();
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
            var allRoleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync(); // Already materialized here

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
                Roles = userRoles.ToList(),
                SelectedRoles = userRoles.ToList(),
                AvailableRolesList = allRoleNames.Select(rName => new SelectListItem // Operates on in-memory list
                {
                    Text = rName,
                    Value = rName,
                    Selected = userRoles.Contains(rName) // userRoles is already List<string>, so Contains is safe
                }).ToList()
            };

            return View(model);
        }

        // POST: Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserViewModel model)
        {
            if (string.IsNullOrEmpty(model.Id))
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(model.Password) && string.IsNullOrEmpty(model.ConfirmPassword))
            {
                ModelState.Remove(nameof(model.Password));
                ModelState.Remove(nameof(model.ConfirmPassword));
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.Email = model.Email;
                user.UserName = model.Email;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.EmployeeId = model.EmployeeId;
                user.IsActiveDriver = model.IsActiveDriver;
                user.DrivingLicenseNumber = model.DrivingLicenseNumber;
                user.LicenseExpiryDate = model.LicenseExpiryDate;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
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
                            // Fix: Materialize all role names first
                            var allRoleNamesForPwdFail = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                            model.AvailableRolesList = allRoleNamesForPwdFail.Select(rName => new SelectListItem
                            {
                                Text = rName,
                                Value = rName,
                                Selected = model.SelectedRoles?.Contains(rName) ?? false // Now safe
                            }).ToList();
                            return View(model);
                        }
                    }

                    var existingRoles = await _userManager.GetRolesAsync(user);
                    var currentSelectedRoles = model.SelectedRoles ?? new List<string>();

                    var rolesToRemove = existingRoles.Except(currentSelectedRoles).ToList();
                    var rolesToAdd = currentSelectedRoles.Except(existingRoles).ToList();

                    if (rolesToRemove.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    }
                    if (rolesToAdd.Any())
                    {
                        await _userManager.AddToRolesAsync(user, rolesToAdd);
                    }

                    TempData["SuccessMessage"] = $"User '{user.Email}' updated successfully.";
                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            // Fix: Materialize all role names first
            var allRoleNamesForModelFail = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            model.AvailableRolesList = allRoleNamesForModelFail.Select(rName => new SelectListItem
            {
                Text = rName,
                Value = rName,
                Selected = model.SelectedRoles?.Contains(rName) ?? false // Now safe
            }).ToList();
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

            if (user.Id == User.FindFirstValue(ClaimTypes.NameIdentifier)!)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Users));
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
            var userViewModelForError = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "N/A",
            };
            var userRolesForError = await _userManager.GetRolesAsync(user);
            userViewModelForError.Roles = userRolesForError.ToList();

            TempData["ErrorMessage"] = "Error deleting user: " + string.Join(", ", result.Errors.Select(e => e.Description));
            return View("DeleteUser", userViewModelForError);
        }
    }
}
