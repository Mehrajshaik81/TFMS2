// Data/SeedData.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using TFMS.Models;
 // For ApplicationUser

namespace TFMS.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Define your roles
            string[] roleNames = { "Fleet Administrator", "Fleet Operator", "Driver" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create a default Fleet Administrator user if one doesn't exist
            string adminEmail = "admin@tfms.com"; // Choose a strong email for your admin
            string adminPassword = "AdminPassword123!"; // Choose a strong password!

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true, // Set to true for immediate login
                    FirstName = "Fleet",
                    LastName = "Admin",
                    EmployeeId = "TFMS-ADM-001",
                    IsActiveDriver = false // Admin is not a driver
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    // Assign the "Fleet Administrator" role to the new admin user
                    await userManager.AddToRoleAsync(adminUser, "Fleet Administrator");
                }
                else
                {
                    // Log errors if user creation failed
                    Console.WriteLine("Error creating admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            // Optionally, create a default Fleet Operator user for testing
            string operatorEmail = "operator@tfms.com";
            string operatorPassword = "OperatorPassword123!";
            if (await userManager.FindByEmailAsync(operatorEmail) == null)
            {
                var operatorUser = new ApplicationUser
                {
                    UserName = operatorEmail,
                    Email = operatorEmail,
                    EmailConfirmed = true,
                    FirstName = "Fleet",
                    LastName = "Operator",
                    EmployeeId = "TFMS-OPT-001",
                    IsActiveDriver = false
                };
                var result = await userManager.CreateAsync(operatorUser, operatorPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(operatorUser, "Fleet Operator");
                }
            }

            // Optionally, create a default Driver user for testing
            string driverEmail = "driver@tfms.com";
            string driverPassword = "DriverPassword123!";
            if (await userManager.FindByEmailAsync(driverEmail) == null)
            {
                var driverUser = new ApplicationUser
                {
                    UserName = driverEmail,
                    Email = driverEmail,
                    EmailConfirmed = true,
                    FirstName = "John",
                    LastName = "Doe",
                    EmployeeId = "TFMS-DRV-001",
                    DrivingLicenseNumber = "DL123456789",
                    LicenseExpiryDate = DateTime.Parse("2030-12-31"),
                    IsActiveDriver = true
                };
                var result = await userManager.CreateAsync(driverUser, driverPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(driverUser, "Driver");
                }
            }
        }
    }
}