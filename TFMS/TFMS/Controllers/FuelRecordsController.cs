// Controllers/FuelRecordsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList
using Microsoft.EntityFrameworkCore;
using TFMS.Models;
using TFMS.Services;
using System.Linq;
using System.Security.Claims; // For ClaimTypes.NameIdentifier
using System.Collections.Generic; // For List

namespace TFMS.Controllers
{
    [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")]
    public class FuelRecordsController : Controller
    {
        private readonly IFuelService _fuelService;
        private readonly IVehicleService _vehicleService;
        private readonly UserManager<ApplicationUser> _userManager;

        public FuelRecordsController(IFuelService fuelService,
                                     IVehicleService vehicleService,
                                     UserManager<ApplicationUser> userManager)
        {
            _fuelService = fuelService;
            _vehicleService = vehicleService;
            _userManager = userManager;
        }

        // GET: FuelRecords
        public async Task<IActionResult> Index(string? searchString, int? vehicleIdFilter, string? driverIdFilter, DateTime? startDate, DateTime? endDate)
        {
            // Store current filter values in ViewBag to persist them in the UI
            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentVehicleFilter = vehicleIdFilter;
            ViewBag.CurrentDriverFilter = driverIdFilter;
            ViewBag.CurrentStartDate = startDate?.ToString("yyyy-MM-dd"); // Format for HTML date input
            ViewBag.CurrentEndDate = endDate?.ToString("yyyy-MM-dd");     // Format for HTML date input

            // Prepare Vehicle filter options
            var allVehicles = await _vehicleService.GetAllVehiclesAsync();
            var vehicleListItems = allVehicles.Select(v => new SelectListItem { Value = v.VehicleId.ToString(), Text = v.RegistrationNumber }).ToList();
            vehicleListItems.Insert(0, new SelectListItem { Value = "0", Text = "All Vehicles" }); // Use "0" for "All" vehicles
            // FIX: Specify dataValueField and dataTextField
            ViewBag.VehicleFilter = new SelectList(vehicleListItems, "Value", "Text", vehicleIdFilter.ToString());

            // Prepare Driver filter options
            var allDrivers = await _userManager.GetUsersInRoleAsync("Driver");
            var driverListItems = allDrivers.Select(d => new SelectListItem { Value = d.Id, Text = d.Email }).ToList();
            driverListItems.Insert(0, new SelectListItem { Value = "All", Text = "All Drivers" }); // Add "All Drivers" option
            // FIX: Specify dataValueField and dataTextField
            ViewBag.DriverFilter = new SelectList(driverListItems, "Value", "Text", driverIdFilter);

            // Fetch fuel records based on filters
            var fuelRecords = await _fuelService.GetAllFuelRecordsAsync(searchString, vehicleIdFilter, driverIdFilter, startDate, endDate);

            // For drivers, filter to only show their fuel records after other filters are applied
            if (User.IsInRole("Driver") && !User.IsInRole("Fleet Administrator") && !User.IsInRole("Fleet Operator"))
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId != null)
                {
                    fuelRecords = fuelRecords.Where(f => f.DriverId == currentUserId);
                }
            }

            return View(fuelRecords);
        }

        // GET: FuelRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fuelRecord = await _fuelService.GetFuelRecordByIdAsync(id.Value);
            if (fuelRecord == null)
            {
                return NotFound();
            }

            return View(fuelRecord);
        }

        // GET: FuelRecords/Create
        public async Task<IActionResult> Create()
        {
            // FIX: Specify dataValueField and dataTextField
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber");

            // For Create, if current user is a driver, pre-select them and show only their ID
            if (User.IsInRole("Driver") && !User.IsInRole("Fleet Administrator") && !User.IsInRole("Fleet Operator"))
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUser = await _userManager.FindByIdAsync(currentUserId);
                // FIX: Specify dataValueField and dataTextField
                ViewBag.DriverId = new SelectList(new List<ApplicationUser> { currentUser }, "Id", "Email", currentUserId);
            }
            else // For Admin/Operator, show all drivers
            {
                // FIX: Specify dataValueField and dataTextField
                ViewBag.DriverId = new SelectList(await _userManager.GetUsersInRoleAsync("Driver"), "Id", "Email");
            }
            return View();
        }

        // POST: FuelRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FuelId,VehicleId,DriverId,Date,FuelQuantity,Cost,OdometerReadingKm,Location")] FuelRecord fuelRecord)
        {
            if (ModelState.IsValid)
            {
                // Ensure driverId is set correctly if only a hidden field is used for drivers
                if (User.IsInRole("Driver") && string.IsNullOrEmpty(fuelRecord.DriverId))
                {
                    fuelRecord.DriverId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                }

                await _fuelService.AddFuelRecordAsync(fuelRecord);
                return RedirectToAction(nameof(Index));
            }
            // If model state is not valid, re-populate ViewBags
            // FIX: Specify dataValueField and dataTextField
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber", fuelRecord.VehicleId);
            if (User.IsInRole("Driver") && !User.IsInRole("Fleet Administrator") && !User.IsInRole("Fleet Operator"))
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUser = await _userManager.FindByIdAsync(currentUserId);
                // FIX: Specify dataValueField and dataTextField
                ViewBag.DriverId = new SelectList(new List<ApplicationUser> { currentUser }, "Id", "Email", currentUserId);
            }
            else
            {
                // FIX: Specify dataValueField and dataTextField
                ViewBag.DriverId = new SelectList(await _userManager.GetUsersInRoleAsync("Driver"), "Id", "Email", fuelRecord.DriverId);
            }
            return View(fuelRecord);
        }

        // GET: FuelRecords/Edit/5
        [Authorize(Roles = "Fleet Administrator")] // Only Admin can edit any fuel record
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fuelRecord = await _fuelService.GetFuelRecordByIdAsync(id.Value);
            if (fuelRecord == null)
            {
                return NotFound();
            }
            // FIX: Specify dataValueField and dataTextField
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber", fuelRecord.VehicleId);
            // FIX: Specify dataValueField and dataTextField
            ViewBag.DriverId = new SelectList(await _userManager.GetUsersInRoleAsync("Driver"), "Id", "Email", fuelRecord.DriverId);
            return View(fuelRecord);
        }

        // POST: FuelRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("FuelId,VehicleId,DriverId,Date,FuelQuantity,Cost,OdometerReadingKm,Location")] FuelRecord fuelRecord)
        {
            if (id != fuelRecord.FuelId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _fuelService.UpdateFuelRecordAsync(fuelRecord);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _fuelService.FuelRecordExistsAsync(fuelRecord.FuelId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            // If model state is not valid, re-populate ViewBags
            // FIX: Specify dataValueField and dataTextField
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber", fuelRecord.VehicleId);
            // FIX: Specify dataValueField and dataTextField
            ViewBag.DriverId = new SelectList(await _userManager.GetUsersInRoleAsync("Driver"), "Id", "Email", fuelRecord.DriverId);
            return View(fuelRecord);
        }

        // GET: FuelRecords/Delete/5
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fuelRecord = await _fuelService.GetFuelRecordByIdAsync(id.Value);
            if (fuelRecord == null)
            {
                return NotFound();
            }

            return View(fuelRecord);
        }

        // POST: FuelRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _fuelService.DeleteFuelRecordAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
